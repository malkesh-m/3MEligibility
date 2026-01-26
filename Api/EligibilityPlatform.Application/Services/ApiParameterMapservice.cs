using AutoMapper;
using MEligibilityPlatform.Application.Services.Inteface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Provides services for mapping API parameters.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ApiParameterMapservice"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class ApiParameterMapservice(IUnitOfWork uow, IMapper mapper) : IApiParameterMapservice
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
        /// Adds a new API parameter mapping to the database.
        /// </summary>
        /// <param name="model">The API parameter mapping data to be added.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(ApiParameterCreateUpdateMapModel model)
        {
            // Maps the model to entity using AutoMapper
            var entity = _mapper.Map<ApiParameterMap>(model);
            // Adds the entity to the repository
            _uow.ApiParameterMapsRepository.Add(entity);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Retrieves all API parameter mappings from the database.
        /// </summary>
        /// <returns>A list of API parameter mappings.</returns>
        public List<ApiParameterListMapModel> GetAll()
        {
            // Retrieves all entities from the repository
            var result = _uow.ApiParameterMapsRepository.GetAll();
            // Maps entities to list models using AutoMapper
            return _mapper.Map<List<ApiParameterListMapModel>>(result);
        }

        /// <summary>
        /// Retrieves an API parameter mapping by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the API parameter mapping.</param>
        /// <returns>The API parameter mapping if found, otherwise null.</returns>
        public ApiParameterListMapModel GetById(int id)
        {
            // Retrieves entity by ID from the repository
            var result = _uow.ApiParameterMapsRepository.GetById(id);
            // Maps entity to list model using AutoMapper
            return _mapper.Map<ApiParameterListMapModel>(result);
        }

        /// <summary>
        /// Removes an API parameter mapping from the database.
        /// </summary>
        /// <param name="id">The unique identifier of the API parameter mapping to be removed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Remove(int id)
        {
            // Retrieves entity by ID from the repository
            var result = _uow.ApiParameterMapsRepository.GetById(id);
            // Removes the entity from the repository
            _uow.ApiParameterMapsRepository.Remove(result);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }
        public List<ApiParameterMapName> GetMappingsByApiId(int apiId)
        {
            var apiParameters = _uow.ApiParametersRepository
                                    .GetAll()
                                    .Where(p => p.ApiId == apiId)
                                    .ToList();

            var parameters = _uow.ParameterRepository.GetAll().ToList();

            var mappings = _uow.ApiParameterMapsRepository
                               .GetAll()
                               .Where(m => apiParameters.Any(p => p.ApiParamterId == m.ApiParameterId))
                               .Select(m => new ApiParameterMapName
                               {
                                   Id = m.Id,
                                   ApiParameterId = m.ApiParameterId,
                                   ParameterId = m.ParameterId,
                                   ApiParameterName = apiParameters
                                                      .FirstOrDefault(p => p.ApiParamterId == m.ApiParameterId)
                                                      ?.ParameterName,
                                   ParameterName = parameters
                                                   .FirstOrDefault(p => p.ParameterId == m.ParameterId)
                                                   ?.ParameterName
                               })
                               .ToList();

            return mappings;
        }

        /// <summary>
        /// Updates an existing API parameter mapping in the database.
        /// </summary>
        /// <param name="model">The updated API parameter mapping data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(ApiParameterCreateUpdateMapModel model)
        {
            // Retrieves existing entity by ID from the repository
            var apiResponce = _uow.ApiParameterMapsRepository.GetById(model.Id);
            // Maps the updated model to entity using AutoMapper
            var entity = _mapper.Map<ApiParameterMap>(apiResponce);
            entity.ApiParameterId = model.ApiParameterId;
            entity.ParameterId = model.ParameterId;

            // Updates the last modification date to current time
            entity.LastModificationDate = DateTime.Now;
            // Updates the entity in the repository
            _uow.ApiParameterMapsRepository.Update(entity);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }
    }
}
