using AutoMapper;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing history EC records.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="HistoryEcService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class HistoryEcService(IUnitOfWork uow, IMapper mapper) : IHistoryEcService
    {
        /// <summary>
        /// The unit of work instance for repository access and persistence.
        /// </summary>
        private readonly IUnitOfWork _uow = uow;

        /// <summary>
        /// The AutoMapper instance for mapping between entity and model types.
        /// </summary>
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Adds a new history EC record to the database.
        /// </summary>
        /// <param name="model">The <see cref="HistoryEcModel"/> containing data to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(HistoryEcModel model)
        {
            // Set update timestamp to current UTC time
            model.UpdatedByDateTime = DateTime.UtcNow;
            // Map model to entity and add to repository
            _uow.HistoryEcRepository.Add(_mapper.Map<HistoryEc>(model));
            // Commit changes asynchronously
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes a history EC record by its identifier.
        /// </summary>
        /// <param name="id">The ID of the record to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the referenced record does not exist.</exception>
        public async Task Delete(int id)
        {
            // Retrieve the history EC item by ID from the repository
            var item = _uow.HistoryEcRepository.GetById(id) ?? throw new ArgumentNullException(nameof(id), "HistoryEc item not found.");
            // Remove the item from the repository
            _uow.HistoryEcRepository.Remove(item);
            // Commit changes asynchronously
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Retrieves all history EC records.
        /// </summary>
        /// <returns>A list of <see cref="HistoryEcModel"/> containing all records.</returns>
        public List<HistoryEcModel> GetAll()
        {
            // Retrieve all history EC records from the repository
            var history = _uow.HistoryEcRepository.GetAll();
            // Map the entity list to model list and return
            return _mapper.Map<List<HistoryEcModel>>(history);
        }

        /// <summary>
        /// Retrieves a history EC record by its identifier.
        /// </summary>
        /// <param name="id">The ID of the record to retrieve.</param>
        /// <returns>The <see cref="HistoryEcModel"/> for the specified ID.</returns>
        public HistoryEcModel GetById(int id)
        {
            // Retrieve the history EC item by ID from the repository
            var history = _uow.HistoryEcRepository.GetById(id);
            // Map the entity to model and return
            return _mapper.Map<HistoryEcModel>(history);
        }

        /// <summary>
        /// Deletes multiple history EC records by their identifiers.
        /// </summary>
        /// <param name="ids">The list of IDs of records to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task MultipleDelete(List<int> ids)
        {
            // Iterate through each ID in the list
            foreach (var id in ids)
            {
                // Retrieve the history EC item by ID from the repository
                var historyItem = _uow.HistoryEcRepository.GetById(id);
                // Check if the item exists
                if (historyItem != null)
                {
                    // Remove the item from the repository
                    _uow.HistoryEcRepository.Remove(historyItem);
                }
            }
            // Commit changes asynchronously
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Updates an existing history EC record.
        /// </summary>
        /// <param name="model">The <see cref="HistoryEcModel"/> containing updated data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the referenced record does not exist.</exception>
        public async Task Update(HistoryEcModel model)
        {
            // Retrieve the history EC item by sequence number from the repository
            var item = _uow.HistoryEcRepository.GetById(model.Seq) ?? throw new ArgumentNullException(nameof(model), "HistoryEc item not found.");
            // Set update timestamp to current UTC time
            model.UpdatedByDateTime = DateTime.UtcNow;
            // Update the entity with model data and persist changes
            _uow.HistoryEcRepository.Update(_mapper.Map<HistoryEcModel, HistoryEc>(model, item));
            // Commit changes asynchronously
            await _uow.CompleteAsync();
        }
    }
}
