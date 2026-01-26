using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Enums;
using MEligibilityPlatform.Infrastructure.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Abstract base repository implementation providing common CRUD operations and maker-checker pattern support.
    /// </summary>
    /// <typeparam name="T">The entity type that this repository manages.</typeparam>
    public abstract class Repository<T> : IRepository<T> where T : class
    {
        // The database context for data operations
        protected readonly EligibilityDbContext _context;
        // The entity set for the generic type
        protected readonly DbSet<T> _entity;
        // The maker-checker entity set
        protected readonly DbSet<MakerChecker> _makerCheckerEntity;
        // Flag indicating if maker-checker pattern is enabled
        private readonly bool isMakerCheckerEnable;
        // Flag indicating if current user is a checker
        private readonly bool isChecker;
        // The current user ID
        private readonly int userId;

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{T}"/> class.
        /// </summary>
        /// <param name="context">The database context used for data operations.</param>
        /// <param name="httpContextAccessor">Provides access to the current HTTP context for user-related data.</param>
        public Repository(EligibilityDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            // Initializes the database context dependency
            _context = context;
            // Sets the entity set for the generic type
            _entity = context.Set<T>();
            // Sets the maker-checker entity set
            _makerCheckerEntity = context.Set<MakerChecker>();

            // Determines if maker-checker pattern is enabled from settings
            isMakerCheckerEnable = context.Set<Setting>().FirstOrDefault()?.IsMakerCheckerEnable ?? false;
            // Extracts user ID from HTTP context claims
            if (httpContextAccessor != null && httpContextAccessor.HttpContext != null)
            {
                userId = int.TryParse(httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value, out int parsedUserId) ? parsedUserId : 0;
            }
            // Gets controller and action names from HTTP context
            var controller = GetControllerName(httpContextAccessor!);
            var action = GetActionName(httpContextAccessor!);
            // Determines if current user is a checker
            isChecker = controller == "MakerChecker" && action == "StatusUpdate";
        }

        /// <summary>
        /// Gets a queryable instance of the entity set.
        /// </summary>
        /// <returns>An <see cref="IQueryable{T}"/> for the entity set.</returns>
        public IQueryable<T> Query() => _entity.AsQueryable();

        /// <summary>
        /// Retrieves all entities from the repository.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> containing all entities.</returns>
        public IEnumerable<T> GetAll() => _entity.AsEnumerable();

        /// <summary>
        /// Retrieves all entities from the repository with optional no-tracking behavior.
        /// </summary>
        /// <param name="asNoTracking">Whether to enable no-tracking for the query.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing all entities.</returns>
        public IEnumerable<T> GetAll(bool asNoTracking) => asNoTracking ? _entity.AsNoTracking().AsEnumerable() : _entity.AsEnumerable();

        /// <summary>
        /// Retrieves an entity by its unique identifier.
        /// </summary>
        /// <param name="id">The ID of the entity to retrieve.</param>
        /// <returns>The entity with the specified ID, or null if not found.</returns>
        public T GetById(int id) => _entity.Find(id)!;

        /// <summary>
        /// Retrieves multiple entities by their IDs.
        /// </summary>
        /// <param name="ids">The collection of IDs to retrieve.</param>
        /// <returns>A list of entities matching the specified IDs.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the primary key cannot be determined for the entity type.</exception>
        public List<T> GetByIds(IList<int> ids)
        {
            // Gets the primary key property name for the entity type
            var keyName = _context.Model.FindEntityType(typeof(T))?
                         .FindPrimaryKey()?
                         .Properties
                         .Select(p => p.Name)
                         .FirstOrDefault();

            // Throws exception if primary key cannot be determined
            if (string.IsNullOrEmpty(keyName))
                throw new InvalidOperationException("Primary key not found for the entity type.");

            // Returns entities matching the specified IDs
            return [.. _entity.Where(e => ids.Contains(EF.Property<int>(e, keyName)))];
        }

        /// <summary>
        /// Adds a new entity to the repository. Uses maker-checker pattern if enabled.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when the entity is null.</exception>
        /// <exception cref="Exception">Thrown when maker-checker pattern is used, indicating the operation was stored for approval.</exception>
        public void Add(T entity, bool skipMakerChecker = false)
        {
            ArgumentNullException.ThrowIfNull(entity);

            if (!skipMakerChecker && ShouldUseMakerChecker(typeof(T).Name))
            {
                SaveToMakerCheckerTable("Add", entity);
            }
            else
            {
                _entity.Add(entity);
                _context.Entry(entity).State = EntityState.Added;
            }
        }

        /// <summary>
        /// Updates an existing entity in the repository. Uses maker-checker pattern if enabled.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <exception cref="ArgumentNullException">Thrown when the entity is null.</exception>
        /// <exception cref="Exception">Thrown when maker-checker pattern is used, indicating the operation was stored for approval.</exception>
        public void Update(T entity)
        {
            // Validates entity parameter
            ArgumentNullException.ThrowIfNull(entity);

            // Gets the primary key property name
            var key = _context.Model.FindEntityType(typeof(T))?
                .FindPrimaryKey()?
                .Properties
                .Select(p => p.Name)
                .FirstOrDefault();

            // Gets the primary key value from the entity
            var keyValue = entity.GetType()?.GetProperty(key!)?.GetValue(entity);

            // Retrieves the original entity for comparison
            var oldEntity = _context.Set<T>()
                .AsNoTracking()
                .FirstOrDefault(e => EF.Property<object>(e, key!).Equals(keyValue));

            // Uses maker-checker pattern if enabled for this entity
            if (ShouldUseMakerChecker(typeof(T).Name))
            {
                // Detaches entity to prevent tracking issues
                _context.Entry(entity).State = EntityState.Detached;

                // Saves operation to maker-checker table for approval
                SaveToMakerCheckerTable("Update", entity, oldEntity!);
            }
            else
            {
                // Attaches entity to the context
                _entity.Attach(entity);
                // Sets entity state to modified
                _context.Entry(entity).State = EntityState.Modified;
            }
        }

        /// <summary>
        /// Removes an entity from the repository. Uses maker-checker pattern if enabled.
        /// </summary>
        /// <param name="entity">The entity to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown when the entity is null.</exception>
        /// <exception cref="Exception">Thrown when maker-checker pattern is used, indicating the operation was stored for approval.</exception>
        public void Remove(T entity)
        {
            // Validates entity parameter
            ArgumentNullException.ThrowIfNull(entity);

            // Uses maker-checker pattern if enabled for this entity
            if (ShouldUseMakerChecker(typeof(T).Name))
            {
                // Saves operation to maker-checker table for approval
                SaveToMakerCheckerTable("Delete", entity, entity);
            }
            else
            {
                // Removes entity from the context
                _entity.Remove(entity);
                // Sets entity state to deleted
                _context.Entry(entity).State = EntityState.Deleted;
            }
        }

        /// <summary>
        /// Removes multiple entities from the repository.
        /// </summary>
        /// <param name="entities">The entities to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown when no entities are provided for deletion.</exception>
        public void RemoveRange(IEnumerable<T> entities)
        {
            // Validates entities parameter
            if (entities == null || !entities.Any())
            {
                throw new ArgumentNullException(nameof(entities), "No entities provided for deletion.");
            }
            // Removes range of entities from the context
            _entity.RemoveRange(entities);
        }

        /// <summary>
        /// Adds multiple entities to the repository.
        /// </summary>
        /// <param name="entities">The entities to add.</param>
        public void AddRange(IEnumerable<T> entities)
        {
            // Adds range of entities to the context
            _entity.AddRange(entities);
        }

        /// <summary>
        /// Saves an operation to the maker-checker table for approval workflow.
        /// </summary>
        /// <param name="actionName">The name of the action (Add, Update, Delete).</param>
        /// <param name="entity">The new entity state.</param>
        /// <param name="oldEntity">The old entity state (for updates and deletes).</param>
        /// <exception cref="Exception">Always thrown to indicate the operation was stored for approval.</exception>
        private void SaveToMakerCheckerTable(string actionName, T entity, T? oldEntity = null)
        {
            // Serializes old entity value for updates and deletes
            var jsonOldValue = actionName == "Add" ? "{}" : JsonConvert.SerializeObject(oldEntity, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            // Serializes new entity value for adds and updates
            var jsonNewValue = actionName == "Delete" ? "{}" : JsonConvert.SerializeObject(entity, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            // Determines target entity for record ID extraction
            object? targetEntity = actionName == "Add" ? entity : oldEntity;

            // Gets the primary key property using KeyAttribute or conventional naming
            var keyProperty = typeof(T).GetProperties()
         .FirstOrDefault(p => Attribute.IsDefined(p, typeof(KeyAttribute)))
         ?? typeof(T).GetProperty("Id")
         ?? typeof(T).GetProperty(typeof(T).Name + "Id");

            // Extracts record ID from the entity
            int recordId = 0;
            if (keyProperty != null && targetEntity != null)
            {
                var keyValue = keyProperty.GetValue(targetEntity);
                if (keyValue != null && int.TryParse(keyValue.ToString(), out int parsedId))
                {
                    recordId = parsedId;
                }
            }
            // Creates maker-checker record
            var makerChecker = new MakerChecker
            {
                TableName = typeof(T).Name,
                ActionName = actionName,
                OldValueJson = jsonOldValue,
                NewValueJson = jsonNewValue,
                Status = (byte)MakerCheckerStatusEnum.Submitted,
                MakerId = userId,
                MakerDate = DateTime.Now,
                RecordId = recordId
            };
            // Adds maker-checker record to context
            _makerCheckerEntity.Add(makerChecker);
            // Saves changes to persist maker-checker record
            _context.SaveChanges();

            // Throws exception to indicate operation requires approval
            throw new MakerCheckerException("Modification has been successfully stored in MakerChecker.");
        }

        /// <summary>
        /// Determines whether the maker-checker pattern should be used for the given entity.
        /// </summary>
        /// <param name="entityName">The name of the entity.</param>
        /// <returns>True if maker-checker should be used; otherwise, false.</returns>
        private bool ShouldUseMakerChecker(string entityName) =>
            // Maker-checker is used if enabled, entity is not excluded, and user is not a checker
            isMakerCheckerEnable && entityName != nameof(EvaluationHistory) && entityName != nameof(User) && entityName != nameof(IntegrationApiEvaluation) && entityName != nameof(MakerChecker) && entityName != nameof(Setting) && entityName != nameof(HistoryPc) && !isChecker;

        /// <summary>
        /// Gets the current controller name from the HTTP context.
        /// </summary>
        /// <param name="httpContext">The HTTP context accessor.</param>
        /// <returns>The controller name, or null if not available.</returns>
        public string? GetControllerName(IHttpContextAccessor httpContext) =>
            // Extracts controller name from route data
            httpContext.HttpContext?.GetRouteData()?.Values["controller"]?.ToString();

        /// <summary>
        /// Gets the current action name from the HTTP context.
        /// </summary>
        /// <param name="httpContext">The HTTP context accessor.</param>
        /// <returns>The action name, or null if not available.</returns>
        public string? GetActionName(IHttpContextAccessor httpContext) =>
            // Extracts action name from route data
            httpContext.HttpContext?.GetRouteData()?.Values["action"]?.ToString();
    }
    public class MakerCheckerException(string message) : Exception(message)
    {
    }
}