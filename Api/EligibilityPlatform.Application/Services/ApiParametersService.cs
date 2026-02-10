using MapsterMapper;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing API parameters.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ApiParametersService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class ApiParametersService(IUnitOfWork uow, IMapper mapper) : IApiParametersService
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
        /// Adds a new API parameter to the database.
        /// </summary>
        /// <param name="model">The API parameter data to be added.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<ApiParameter> Add(ApiParametersCreateUpdateModel model)
        {
            var entity = _mapper.Map<ApiParameter>(model);
            entity.CreatedByDateTime = DateTime.Now;

            _uow.ApiParametersRepository.Add(entity);
            await _uow.CompleteAsync();

            return entity; // <-- return entity with generated ID
        }

        /// <summary>
        /// Adds multiple API parameters to the database in a batch.
        /// </summary>
        /// <param name="models">A list of API parameter data to be added.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddRange(List<ApiParametersCreateUpdateModel> models)
        {
            // Maps the list of models to entities using AutoMapper
            var entities = _mapper.Map<List<ApiParameter>>(models);

            // Sets creation date time for each entity
            foreach (var entity in entities)
            {
                // Sets the creation date time to current time for each entity
                entity.CreatedByDateTime = DateTime.Now;
            }

            // Adds all entities to the repository in a batch
            _uow.ApiParametersRepository.AddRange(entities);

            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes API parameters by the associated API identifier.
        /// </summary>
        /// <param name="apiId">The API identifier.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteByApiIdAsync(int apiId)
        {
            // Queries for existing parameters with the specified API ID
            var existingParams = _uow.ApiParametersRepository.Query()
            // Filters parameters by API ID
            .Where(p => p.ApiId == apiId);

            // Removes all matching parameters from the repository
            _uow.ApiParametersRepository.RemoveRange(existingParams);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Retrieves all API parameters from the database.
        /// </summary>
        /// <returns>A list of API parameters.</returns>
        public List<ApiParametersListModel> GetAll(int tenantId)
        {
            // Retrieves all entities from the repository
            var result = _uow.ApiParametersRepository.GetAllByTenantId(tenantId);
            // Maps entities to list models using AutoMapper
            return _mapper.Map<List<ApiParametersListModel>>(result);
        }

        /// <summary>
        /// Retrieves an API parameter by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the API parameter.</param>
        /// <returns>The API parameter if found, otherwise null.</returns>
        public ApiParametersListModel GetById(int id)
        {
            // Retrieves entity by ID from the repository
            var result = _uow.ApiParametersRepository.GetById(id);
            // Maps entity to list model using AutoMapper
            return _mapper.Map<ApiParametersListModel>(result);
        }
        public async Task<List<ApiParametersListModel>> GetByApiId(int id)
        {
            // Retrieves entity by ID from the repository
            var result = await _uow.ApiParametersRepository.Query().Where(p => p.ApiId == id).ToListAsync();
            // Maps entity to list model using AutoMapper
            var data = _mapper.Map<List<ApiParametersListModel>>(result);
            return data;
        }

        /// <summary>
        /// Removes an API parameter from the database.
        /// </summary>
        /// <param name="id">The unique identifier of the API parameter to be removed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Remove(int id)
        {
            // Retrieves entity by ID from the repository
            var ApiParameter = _uow.ApiParametersRepository.GetById(id);
            // Removes the entity from the repository
            _uow.ApiParametersRepository.Remove(ApiParameter);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        public async Task RemoveMultiple(List<int> ids)
        {
            foreach (int id in ids)
            {
                // Retrieves entity by ID from the repository
                var ApiParameter = _uow.ApiParametersRepository.GetById(id);
                if (ApiParameter != null)
                {
                    _uow.ApiParametersRepository.Remove(ApiParameter);
                }

            }
            // Removes the entity from the repository
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Updates an existing API parameter in the database.
        /// </summary>
        /// <param name="model">The updated API parameter data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(ApiParametersCreateUpdateModel model)
        {
            // Retrieves existing entity by ID from the repository
            //var ApiParameter = _uow.ApiParametersRepository.GetById(model.ApiParamterId);
            // Maps the updated model to entity using AutoMapper
            var entity = _mapper.Map<ApiParameter>(model);
            // Sets the update date time to current time
            entity.UpdatedByDateTime = DateTime.Now;
            // Updates the entity in the repository
            _uow.ApiParametersRepository.Update(entity);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }
    }
}
