using MapsterMapper;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionService"/> class.
    /// </summary>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class ConditionService(IUnitOfWork uow, IMapper mapper) : IConditionService
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Adds a new condition to the repository.
        /// </summary>
        /// <param name="model">The condition model to be added.</param>
        public async Task Add(ConditionModel model)
        {
            // Sets the update timestamp to current UTC time
            model.UpdatedByDateTime = DateTime.UtcNow;
            // Maps the model to Condition entity and adds it to the repository
            _uow.ConditionRepository.Add(_mapper.Map<Condition>(model));
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes a condition from the repository based on the given ID.
        /// </summary>
        /// <param name="id">The ID of the condition to delete.</param>
        public async Task Delete(int id)
        {
            // Retrieves the condition entity by ID from the repository
            var item = _uow.ConditionRepository.GetById(id);
            // Maps the entity to Condition and removes it from the repository
            _uow.ConditionRepository.Remove(_mapper.Map<Condition>(item));
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Retrieves all conditions from the repository.
        /// </summary>
        /// <returns>A list of all conditions mapped to ConditionModel.</returns>
        public List<ConditionModel> GetAll()
        {
            // Retrieves all condition entities from the repository
            var conditions = _uow.ConditionRepository.GetAll();
            // Maps the condition entities to ConditionModel objects and returns the list
            return _mapper.Map<List<ConditionModel>>(conditions);
        }

        /// <summary>
        /// Retrieves a single condition by ID.
        /// </summary>
        /// <param name="id">The ID of the condition to retrieve.</param>
        /// <returns>The condition model mapped from the entity.</returns>
        public ConditionModel GetById(int id)
        {
            // Retrieves the condition entity by ID from the repository
            var condition = _uow.ConditionRepository.GetById(id);
            // Maps the condition entity to ConditionModel object and returns it
            return _mapper.Map<ConditionModel>(condition);
        }

        /// <summary>
        /// Deletes multiple conditions from the repository.
        /// </summary>
        /// <param name="ids">The list of condition IDs to delete.</param>
        public async Task MultipleDelete(List<int> ids)
        {
            // Iterates through each condition ID in the list
            foreach (var id in ids)
            {
                // Retrieves the condition entity by ID from the repository
                var conditionitem = _uow.ConditionRepository.GetById(id);
                // Checks if the condition entity exists
                if (conditionitem != null)
                {
                    // Removes the condition entity from the repository
                    _uow.ConditionRepository.Remove(conditionitem);
                }
            }
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Updates an existing condition with new values.
        /// </summary>
        /// <param name="model">The condition model containing updated values.</param>
        public async Task Update(ConditionModel model)
        {
            // Retrieves the existing condition entity by ID from the repository
            var Item = _uow.ConditionRepository.GetById(model.ConditionId);
            // Sets the update timestamp to current UTC time
            model.UpdatedByDateTime = DateTime.UtcNow;
            // Maps the updated model to the existing entity and updates it in the repository
            _uow.ConditionRepository.Update(_mapper.Map<ConditionModel, Condition>(model, Item));
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }
    }
}
