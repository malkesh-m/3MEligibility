using AutoMapper;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Application.UnitOfWork;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing parameter map operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ParamtersMapService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class ParamtersMapService(IUnitOfWork uow, IMapper mapper) : IParamtersMapService
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
        /// Adds a new parameter map to the database.
        /// </summary>
        /// <param name="model">The <see cref="ParamtersMapModel"/> containing the data to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(ParamtersMapModel model)
        {
            // Nullify MapFunctionId if zero
            model.MapFunctionId = model.MapFunctionId == 0 ? (int?)null : model.MapFunctionId;
            // Set current UTC time on update timestamp
            model.UpdatedByDateTime = DateTime.UtcNow;
            // Map model to entity and add to repository
            _uow.ParamtersMapRepository.Add(_mapper.Map<ParamtersMap>(model));
            // Persist changes to database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes a parameter map by API ID, node ID, and parameter ID.
        /// </summary>
        /// <param name="apiId">The API ID.</param>
        /// <param name="nodeId">The node ID.</param>
        /// <param name="parameterId">The parameter ID.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Delete(int apiId, int nodeId, int parameterId)
        {
            // Query repository for matching parameter map
            var paramtersMaps = await _uow.ParamtersMapRepository.Query()
                .FirstOrDefaultAsync(pm => pm.Apiid == apiId && pm.NodeId == nodeId && pm.ParameterId == parameterId);
            // Remove found entity from repository
            if (paramtersMaps != null)
            {
                _uow.ParamtersMapRepository.Remove(paramtersMaps);
            }
            // Persist removal
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Gets all parameter maps.
        /// </summary>
        /// <returns>A list of <see cref="ParamtersMapModel"/> representing all parameter maps.</returns>
        public List<ParamtersMapModel> GetAll()
        {
            // Retrieve all parameter map entities
            var paramtersMaps = _uow.ParamtersMapRepository.GetAll();
            // Map entities to model list
            return _mapper.Map<List<ParamtersMapModel>>(paramtersMaps);
        }

        /// <summary>
        /// Gets a parameter map by API ID, node ID, and parameter ID.
        /// </summary>
        /// <param name="apiId">The API ID.</param>
        /// <param name="nodeId">The node ID.</param>
        /// <param name="parameterId">The parameter ID.</param>
        /// <returns>The <see cref="ParamtersMapModel"/> for the specified identifiers.</returns>
        public ParamtersMapModel GetById(int apiId, int nodeId, int parameterId)
        {
            // Query for the specific parameter map entity
            var paramtersMaps = _uow.ParamtersMapRepository.Query()
                .FirstOrDefault(pm => pm.Apiid == apiId && pm.NodeId == nodeId && pm.ParameterId == parameterId);
            // Map entity to model
            return _mapper.Map<ParamtersMapModel>(paramtersMaps);
        }

        /// <summary>
        /// Updates an existing parameter map.
        /// </summary>
        /// <param name="model">The <see cref="ParamtersMapModel"/> containing updated data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when the specified record is not found.</exception>
        public async Task Update(ParamtersMapModel model)
        {
            // Query for existing parameter map entity
            var paramtersMaps = await _uow.ParamtersMapRepository.Query()
                .FirstOrDefaultAsync(pm =>
                    pm.Apiid == model.Apiid &&
                    pm.NodeId == model.NodeId &&
                    pm.ParameterId == model.ParameterId) ?? throw new Exception("Record not found");
            // Set update timestamp
            model.UpdatedByDateTime = DateTime.UtcNow;
            // Map updated model onto existing entity and update repository
            _uow.ParamtersMapRepository.Update(
                _mapper.Map<ParamtersMapModel, ParamtersMap>(model, paramtersMaps));
            // Persist changes
            await _uow.CompleteAsync();
        }
    }
}
