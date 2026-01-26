using AutoMapper;
using MEligibilityPlatform.Application.Services.Inteface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing HistoryEr records.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="HistoryErService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class HistoryErService(IUnitOfWork uow, IMapper mapper) : IHistoryErService
    {
        /// <summary>The unit of work instance for repository and transaction operations.</summary>
        private readonly IUnitOfWork _uow = uow;

        /// <summary>The AutoMapper instance for mapping between entity and model.</summary>
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Adds a new <see cref="HistoryErModel"/> record to the database asynchronously.
        /// </summary>
        /// <param name="model">The model with data to insert.</param>
        /// <returns>A task that represents the add operation.</returns>
        public async Task Add(HistoryErModel model)
        {
            // Record UTC timestamp before save
            model.UpdatedByDateTime = DateTime.UtcNow;

            // Map model to domain entity and queue for insertion
            _uow.HistoryErRepository.Add(_mapper.Map<HistoryEr>(model));

            // Commit transaction
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes a <see cref="HistoryErModel"/> specified by its ID asynchronously.
        /// </summary>
        /// <param name="id">The identifier of the record to delete.</param>
        /// <returns>A task that represents the delete operation.</returns>
        public async Task Delete(int id)
        {
            var item = _uow.HistoryErRepository.GetById(id);

            // Remove only if exists
            if (item != null)
            {
                _uow.HistoryErRepository.Remove(item);
                await _uow.CompleteAsync();
            }
        }

        /// <summary>
        /// Retrieves all <see cref="HistoryErModel"/> records.
        /// </summary>
        /// <returns>A list of all records mapped to models.</returns>
        public List<HistoryErModel> GetAll()
        {
            var historyErs = _uow.HistoryErRepository.GetAll();
            return _mapper.Map<List<HistoryErModel>>(historyErs);
        }

        /// <summary>
        /// Retrieves a <see cref="HistoryErModel"/> record by its ID.
        /// </summary>
        /// <param name="id">The ID of the record to retrieve.</param>
        /// <returns>The matching model, or null if not found.</returns>
        public HistoryErModel GetById(int id)
        {
            var historyErs = _uow.HistoryErRepository.GetById(id);
            return _mapper.Map<HistoryErModel>(historyErs);
        }

        /// <summary>
        /// Updates an existing <see cref="HistoryErModel"/> record asynchronously.
        /// </summary>
        /// <param name="model">The model containing updated values.</param>
        /// <returns>A task representing the update operation.</returns>
        public async Task Update(HistoryErModel model)
        {
            var item = _uow.HistoryErRepository.GetById(model.Seq);

            // Skip update if not exist
            if (item != null)
            {
                model.UpdatedByDateTime = DateTime.UtcNow;
                _uow.HistoryErRepository.Update(
                    _mapper.Map<HistoryErModel, HistoryEr>(model, item)
                );
                await _uow.CompleteAsync();
            }
        }

        /// <summary>
        /// Deletes multiple <see cref="HistoryErModel"/> records by their IDs.
        /// </summary>
        /// <param name="ids">A list of IDs to delete.</param>
        /// <returns>A task representing the multiple delete operation.</returns>
        public async Task MultipleDelete(List<int> ids)
        {
            // Remove each existing record if found
            foreach (var id in ids)
            {
                var historyItem = _uow.HistoryErRepository.GetById(id);
                if (historyItem != null)
                {
                    _uow.HistoryErRepository.Remove(historyItem);
                }
            }

            // Commit all removals
            await _uow.CompleteAsync();
        }
    }
}
