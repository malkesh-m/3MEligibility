using MapsterMapper;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for handling audit operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AuditService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class AuditService(IUnitOfWork uow, IMapper mapper) : IAuditService
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
        /// Adds a new audit record.
        /// </summary>
        /// <param name="model">The audit model containing details of the audit record.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(AuditCreateUpdateModel model)
        {
            // Sets the update date time to current UTC time
            model.UpdatedByDateTime = DateTime.UtcNow;
            // Maps the model to entity using AutoMapper and adds it to the repository
            _uow.AuditRepository.Add(_mapper.Map<Audit>(model));
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes an audit record by its ID.
        /// </summary>
        /// <param name="id">The ID of the audit record to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Delete(int id)
        {
            // Retrieves the audit entity by ID from the repository
            var Item = _uow.AuditRepository.GetById(id);
            // Removes the entity from the repository
            _uow.AuditRepository.Remove(Item);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Retrieves all audit records with pagination support.
        /// </summary>
        /// <param name="pageIndex">The zero-based index of the page to retrieve.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <returns>A pagination model containing audit records.</returns>
        public async Task<PaginationModel<AuditModel>> GetAll(int tenantId,int pageIndex = 0, int pageSize = 10)
        {
            // Creates a queryable for audit entities with no tracking for read-only operations
            var logsQuery = _uow.AuditRepository.Query().Where(a=>a.TenantId==tenantId).AsNoTracking();
            // Counts the total number of records in the query
            var totalCount = await logsQuery.CountAsync();
            // Retrieves a paginated list of audit records, ordered by action date descending
            var logs = await logsQuery
                .OrderByDescending(l => l.ActionDate)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
            // Maps the entity list to model list using AutoMapper
            var Log = _mapper.Map<List<AuditModel>>(logs);
            // Returns a pagination model with the results
            return new PaginationModel<AuditModel>
            {
                // Indicates the operation was successful
                IsSuccess = true,
                // Contains the paginated audit data
                Data = Log,
                // Contains the total count of records
                TotalCount = totalCount
            };
        }

        /// <summary>
        /// Retrieves an audit record by its ID.
        /// </summary>
        /// <param name="id">The ID of the audit record.</param>
        /// <returns>The audit model if found; otherwise, null.</returns>
        public AuditModel GetById(int id,int tenantId)
        {
            // Retrieves the audit entity by ID from the repository
            var Auditval = _uow.AuditRepository.Query().Where(a=>a.AuditId==id && a.TenantId==tenantId );
            // Maps the entity to model using AutoMapper and returns the result
            return _mapper.Map<AuditModel>(Auditval);
        }

        /// <summary>
        /// Deletes multiple audit records by their IDs.
        /// </summary>
        /// <param name="ids">A list of audit record IDs to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task MultiPleDelete(List<int> ids)
        {
            // Iterates through each ID in the list
            foreach (var id in ids)
            {
                // Retrieves the audit entity by ID from the repository
                var audits = _uow.AuditRepository.GetById(id);
                // Checks if the entity exists
                if (audits != null)
                {
                    // Removes the entity from the repository
                    _uow.AuditRepository.Remove(audits);
                }
            }
            // Commits all changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Updates an existing audit record.
        /// </summary>
        /// <param name="model">The updated audit model.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(AuditCreateUpdateModel model)
        {
            // Retrieves the existing entity by ID from the repository
            var Item = _uow.AuditRepository.GetById(model.AuditId);
            // Sets the update date time to current UTC time
            model.UpdatedByDateTime = DateTime.UtcNow;
            // Updates the entity with values from the model using AutoMapper
            _uow.AuditRepository.Update(_mapper.Map<AuditCreateUpdateModel, Audit>(model, Item));
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }
    }
}

