using MapsterMapper;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing <see cref="HistoryParameter"/> records.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="HistoryParameterService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class HistoryParameterService(IUnitOfWork uow, IMapper mapper) : IHistoryParameterService
    {
        /// <summary>
        /// The unit of work instance for repository and transaction operations.
        /// </summary>
        private readonly IUnitOfWork _uow = uow;

        /// <summary>
        /// The AutoMapper instance for mapping between entities and models.
        /// </summary>
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Adds a new <see cref="HistoryParameterModel"/> record to the database asynchronously.
        /// </summary>
        /// <param name="model">
        /// The model instance containing the data to add.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous add operation.</returns>
        public async Task Add(HistoryParameterModel model)
        {
            // Record UTC timestamp before saving for audit purposes
            model.UpdatedByDateTime = DateTime.UtcNow;

            // Map model to entity and queue it for insertion
            _uow.HistoryParameterRepository.Add(_mapper.Map<HistoryParameter>(model));

            // Persist changes to database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes a <see cref="HistoryParameterModel"/> record identified by its ID asynchronously.
        /// </summary>
        /// <param name="id">The identifier of the record to delete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous delete operation.</returns>
        public async Task Delete(int id)
        {
            var item = _uow.HistoryParameterRepository.GetById(id);
            if (item != null)
            {
                _uow.HistoryParameterRepository.Remove(item);
                await _uow.CompleteAsync();
            }
        }

        /// <summary>
        /// Retrieves all <see cref="HistoryParameterModel"/> records.
        /// </summary>
        /// <returns>
        /// A list of all records mapped to <see cref="HistoryParameterModel"/>.
        /// </returns>
        public List<HistoryParameterModel> GetAll()
        {
            var all = _uow.HistoryParameterRepository.GetAll();
            return _mapper.Map<List<HistoryParameterModel>>(all);
        }

        /// <summary>
        /// Retrieves a <see cref="HistoryParameterModel"/> record by its ID.
        /// </summary>
        /// <param name="id">The ID of the record to retrieve.</param>
        /// <returns>
        /// The matching <see cref="HistoryParameterModel"/>, or null if not found.
        /// </returns>
        public HistoryParameterModel GetById(int id)
        {
            var entity = _uow.HistoryParameterRepository.GetById(id);
            return _mapper.Map<HistoryParameterModel>(entity);
        }

        /// <summary>
        /// Updates an existing <see cref="HistoryParameterModel"/> record asynchronously.
        /// </summary>
        /// <param name="model">The model containing updated values.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous update operation.</returns>
        public async Task Update(HistoryParameterModel model)
        {
            var item = _uow.HistoryParameterRepository.GetById(model.Seq);
            if (item != null)
            {
                model.UpdatedByDateTime = DateTime.UtcNow;
                _uow.HistoryParameterRepository.Update(
                    _mapper.Map<HistoryParameterModel, HistoryParameter>(model, item)
                );
                await _uow.CompleteAsync();
            }
        }

        /// <summary>
        /// Deletes multiple <see cref="HistoryParameterModel"/> records by their IDs asynchronously.
        /// </summary>
        /// <param name="ids">
        /// A list of IDs representing records to delete.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the multiple delete operation.</returns>
        public async Task MultipleDelete(List<int> ids)
        {
            // Loop through each ID and remove if entity exists
            foreach (var id in ids)
            {
                var historyItem = _uow.HistoryParameterRepository.GetById(id);
                if (historyItem != null)
                {
                    _uow.HistoryParameterRepository.Remove(historyItem);
                }
            }

            // Commit all deletions at once
            await _uow.CompleteAsync();
        }
    }

}
