using AutoMapper;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Application.UnitOfWork;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing exception management operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ExceptionManagementService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class ExceptionManagementService(IUnitOfWork uow, IMapper mapper) : IExceptionManagementService
    {
        /// <summary>
        /// The unit of work instance for database operations.
        /// </summary>
        private readonly IUnitOfWork _uow = uow;

        /// <summary>
        /// The AutoMapper instance for object mapping.
        /// </summary>
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Adds a new exception management record to the database.
        /// Maps the incoming model to the corresponding entity and sets the created and updated timestamps.
        /// Validates that Start Date and End Date are provided for temporary exceptions.
        /// </summary>
        /// <param name="entityId">The entity ID to associate with the exception.</param>
        /// <param name="model">The model containing the data to be added to the ExceptionManagement entity.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when Start Date and End Date are not provided for temporary exceptions.</exception>
        /// <exception cref="Exception">Thrown when an error occurs during the add operation.</exception>
        public async Task Add(int entityId, ExceptionManagementCreateOrUpdateModel model)
        {
            // Validates that temporary exceptions have both start and end dates specified
            if (model.IsTemporary == true)
            {
                if (model.StartDate == null || model.EndDate == null)
                {
                    throw new InvalidOperationException("Start Date and End Date are required for temporary exceptions.");
                }
            }

            // Maps the incoming model to the ExceptionManagement entity using AutoMapper
            var entity = _mapper.Map<ExceptionManagement>(model);
            // Sets creation and update timestamps to current server time
            entity.CreatedByDateTime = DateTime.Now;
            entity.UpdatedByDateTime = DateTime.Now;
            // Associates the exception with the specified entity ID
            entity.TenantId = entityId;

            // Processes and adds associated products if any are specified in the model
            if (model.ProductId != null && model.ProductId.Count != 0)
            {
                foreach (var productId in model.ProductId)
                {
                    // Creates new ExceptionProduct relationships for each product ID
                    entity.ExceptionProducts.Add(new ExceptionProduct { ProductId = productId });
                }
            }

            // Adds the new exception entity to the repository
            _uow.ExceptionManagementRepository.Add(entity);
            // Commits all changes to the database asynchronously
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes an existing exception management record from the database by its ID.
        /// Retrieves the record by its ID and entity ID, removes it, and commits the changes asynchronously.
        /// </summary>
        /// <param name="entityId">The entity ID associated with the exception.</param>
        /// <param name="id">The ID of the exception management record to be deleted.</param>
        /// <returns>A Task representing the asynchronous delete operation.</returns>
        /// <exception cref="Exception">Thrown when the exception record is not found or an error occurs during the delete operation.</exception>
        public async Task Delete(int entityId, int id)
        {
            // Queries the repository to find the exception by ID and entity ID
            var exception = _uow.ExceptionManagementRepository.Query().First(f => f.ExceptionManagementId == id && f.TenantId == entityId);
            if (exception != null)
            {

                // Marks the entity for removal in the repository
                _uow.ExceptionManagementRepository.Remove(exception);

                var exceptionProducts = await _uow.ExceptionProductRepository.Query().Where(p => p.ExceptionManagementId == id).ToListAsync();
                if (exceptionProducts.Count != 0)
                    _uow.ExceptionProductRepository.RemoveRange(exceptionProducts);
                // Commits the deletion to the database asynchronously
                await _uow.CompleteAsync();
            }
        }

        /// <summary>
        /// Retrieves all exception management records from the database for a specific entity.
        /// Maps the data to a list of ExceptionManagementListModel objects for easier presentation.
        /// </summary>
        /// <param name="entityId">The entity ID to filter exception records.</param>
        /// <returns>A list of ExceptionManagementListModel objects containing all exception management records for the specified entity.</returns>
        public List<ExceptionManagementListModel> GetAll(int entityId)
        {
            // Filters exceptions by the specified entity ID
            var exceptions = _uow.ExceptionManagementRepository.Query().Where(f => f.TenantId == entityId);
            // Maps the entity collection to a list of view models using AutoMapper
            return _mapper.Map<List<ExceptionManagementListModel>>(exceptions);
        }

        /// <summary>
        /// Retrieves a specific exception management record by its ID and entity ID.
        /// Includes associated exception products and maps the retrieved entity to an ExceptionManagementGetModel object.
        /// </summary>
        /// <param name="entityId">The entity ID associated with the exception.</param>
        /// <param name="id">The ID of the exception management record to retrieve.</param>
        /// <returns>The ExceptionManagementGetModel object representing the exception management record with the specified ID.</returns>
        /// <exception cref="Exception">Thrown when the exception record is not found.</exception>
        public ExceptionManagementGetModel GetById(int entityId, int id)
        {
            // Queries the repository for the specific exception, including related products via eager loading
            var exception = _uow.ExceptionManagementRepository
                .Query()
                .Include(e => e.ExceptionProducts)
                .First(f => f.ExceptionManagementId == id && f.TenantId == entityId);

            // Maps the entity to the detailed view model
            var model = _mapper.Map<ExceptionManagementGetModel>(exception);

            // Manually extracts product IDs from the related products collection
            model.ProductId = [.. exception.ExceptionProducts.Select(p => p.ProductId)];

            return model;
        }

        /// <summary>
        /// Updates an existing exception management record in the database.
        /// Retrieves the record by its ID and entity ID, updates associated products, 
        /// maps the updated data from the provided model, and saves the changes asynchronously.
        /// </summary>
        /// <param name="entityId">The entity ID associated with the exception.</param>
        /// <param name="model">The model containing the updated data for the exception management record.</param>
        /// <returns>A Task representing the asynchronous update operation.</returns>
        /// <exception cref="Exception">Thrown when the exception record is not found or an error occurs during the update operation.</exception>
        public async Task Update(int entityId, ExceptionManagementCreateOrUpdateModel model)
        {
            // Retrieves the existing exception entity from the repository
            var entity = _uow.ExceptionManagementRepository.Query().FirstOrDefault(f => f.ExceptionManagementId == model.ExceptionManagementId && f.TenantId == entityId);

            // Fetches all existing exception products associated with this exception
            var exceptionProducts = _uow.ExceptionProductRepository.Query()
                .Where(p => p.ExceptionManagementId == model.ExceptionManagementId)
                .ToList();

            // Normalizes the incoming product IDs (distinct list or empty list if null)
            var modelProductIds = model.ProductId?.Distinct().ToList() ?? [];

            // Extracts existing product IDs from the current relationship entities
            var existingProductIds = exceptionProducts.Select(p => p.ProductId).ToList();

            // Identifies new products to add (present in model but not in database)
            var productsToAdd = modelProductIds
                .Where(id => !existingProductIds.Contains(id))
                .Select(id => new ExceptionProduct
                {
                    ProductId = id,
                    ExceptionManagementId = model.ExceptionManagementId,
                })
                .ToList();

            // Identifies existing products to remove (present in database but not in model)
            var productsToRemove = exceptionProducts
                .Where(p => !modelProductIds.Contains(p.ProductId))
                .ToList();

            // Removes obsolete product relationships
            if (productsToRemove.Count != 0)
            {
                _uow.ExceptionProductRepository.RemoveRange(productsToRemove);
            }

            // Adds new product relationships
            if (productsToAdd.Count != 0)
            {
                _uow.ExceptionProductRepository.AddRange(productsToAdd);
            }

            // Updates the timestamp to reflect when this modification occurred
            entity!.UpdatedByDateTime = DateTime.Now;
            // Ensures the entity ID remains consistent with the update request
            model.TenantId = entityId;
            // Maps updated values from model to existing entity using AutoMapper
            _uow.ExceptionManagementRepository.Update(_mapper.Map<ExceptionManagementCreateOrUpdateModel, ExceptionManagement>(model, entity));
            // Commits all changes (entity update and product relationships) to the database
            await _uow.CompleteAsync();
        }
    }
}

