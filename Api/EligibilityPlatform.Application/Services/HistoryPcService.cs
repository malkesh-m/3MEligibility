using AutoMapper;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing HistoryPc operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="HistoryPcService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class HistoryPcService(IUnitOfWork uow, IMapper mapper) : IHistoryPcService
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
        /// Adds a new HistoryPc record to the database.
        /// </summary>
        /// <param name="model">The HistoryPcModel containing the data to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(HistoryPcModel model)
        {
            // Sets the update timestamp to current UTC time
            model.UpdatedByDateTime = DateTime.UtcNow;
            // Maps the incoming model to HistoryPc entity
            _uow.HistoryPcRepository.Add(_mapper.Map<HistoryPc>(model));

            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes a HistoryPc record by its entity ID and transaction ID.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="id">The transaction ID of the HistoryPc record to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Delete(int tenantId, int id)
        {
            // Retrieves the HistoryPc record by entity ID and transaction ID
            var Item = _uow.HistoryPcRepository.Query().First(f => f.TranId == id && f.TenantId == tenantId);
            // Removes the record from the repository
            _uow.HistoryPcRepository.Remove(Item);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Gets all HistoryPc records for a specific entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>A list of HistoryPcModel representing all records for the entity.</returns>
        public List<HistoryPcModel> GetAll(int tenantId)
        {
            // Retrieves all HistoryPc records for the specified entity
            var HistoriPcs = _uow.HistoryPcRepository.Query().Where(f => f.TenantId == tenantId);
            // Maps the records to HistoryPcModel objects
            return _mapper.Map<List<HistoryPcModel>>(HistoriPcs);
        }

        /// <summary>
        /// Gets a HistoryPc record by its entity ID and transaction ID.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="id">The transaction ID of the HistoryPc record to retrieve.</param>
        /// <returns>The HistoryPcModel for the specified entity and transaction ID.</returns>
        public HistoryPcModel GetById(int tenantId, int id)
        {
            // Retrieves the specific HistoryPc record by entity ID and transaction ID
            var city = _uow.HistoryPcRepository.Query().First(f => f.TranId == id && f.TenantId == tenantId);
            // Maps the record to HistoryPcModel object
            return _mapper.Map<HistoryPcModel>(city);
        }

        /// <summary>
        /// Updates an existing HistoryPc record.
        /// </summary>
        /// <param name="model">The HistoryPcModel containing updated data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(HistoryPcModel model)
        {
            // Retrieves the existing HistoryPc record by entity ID and transaction ID
            var Item = _uow.HistoryPcRepository.Query().First(f => f.TranId == model.TranId && f.TenantId == model.TenantId);
            // Sets the update timestamp to current UTC time
            model.UpdatedByDateTime = DateTime.UtcNow;
            // Updates the record with mapped data from the model
            _uow.HistoryPcRepository.Update(_mapper.Map<HistoryPcModel, HistoryPc>(model, Item));
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes multiple HistoryPc records for a specific entity by their transaction IDs.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="ids">A list of transaction IDs to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task MultipleDelete(int tenantId, List<int> ids)
        {
            // Validates that all provided IDs exist for the given entity
            foreach (var id in ids)
            {
                // Checks if the record exists in the database
                var hasvalue = await _uow.HistoryPcRepository.Query().AnyAsync(item => item.TranId == id && item.TenantId == tenantId);
                // Throws exception if any ID is not found
                if (hasvalue == false)
                {
                    throw new Exception($"these  id's: {id} is not present. please provide valid id. ");
                }
            }

            // Deletes all validated records
            foreach (var id in ids)
            {
                // Retrieves each record by entity ID and transaction ID
                var historypcitem = _uow.HistoryPcRepository.Query().First(f => f.TranId == id && f.TenantId == tenantId);
                // Removes the record if found
                if (historypcitem != null)
                {
                    _uow.HistoryPcRepository.Remove(historypcitem);
                }
            }

            // Commits all deletion changes to the database
            await _uow.CompleteAsync();
        }
    }
}
