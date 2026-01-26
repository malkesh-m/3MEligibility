using AutoMapper;
using MEligibilityPlatform.Application.Services.Inteface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Provides services for managing API responses.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ApiResponsesService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class ApiResponsesService(IUnitOfWork uow, IMapper mapper) : IApiResponsesService
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
        /// Adds a new API response to the database.
        /// </summary>
        /// <param name="model">The API response data to be added.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(ApiResponsesCreateUpdateModel model)
        {
            // Maps the model to entity using AutoMapper
            var entity = _mapper.Map<ApiResponse>(model);
            // Sets the creation date time to current time
            entity.CreatedByDateTime = DateTime.Now;
            // Adds the entity to the repository
            _uow.ApiResponsesRepository.Add(entity);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Adds multiple API responses to the database in a batch.
        /// </summary>
        /// <param name="models">A list of API response data to be added.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddRange(List<ApiResponsesCreateUpdateModel> models)
        {
            // Maps the list of models to entities using AutoMapper
            var entities = _mapper.Map<List<ApiResponse>>(models);

            // Sets creation date time for each entity
            foreach (var entity in entities)
            {
                // Sets the creation date time to current time for each entity
                entity.CreatedByDateTime = DateTime.Now;
            }

            // Adds all entities to the repository in a batch
            _uow.ApiResponsesRepository.AddRange(entities);

            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Retrieves all API responses from the database.
        /// </summary>
        /// <returns>A list of API responses.</returns>
        public List<ApiResponsesListModel> GetAll()
        {
            // Retrieves all entities from the repository
            var result = _uow.ApiResponsesRepository.GetAll();
            // Maps entities to list models using AutoMapper
            return _mapper.Map<List<ApiResponsesListModel>>(result);
        }

        /// <summary>
        /// Retrieves an API response by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the API response.</param>
        /// <returns>The API response if found, otherwise null.</returns>
        public ApiResponsesListModel GetById(int id)
        {
            // Retrieves entity by ID from the repository
            var result = _uow.ApiResponsesRepository.GetById(id);
            // Maps entity to list model using AutoMapper
            return _mapper.Map<ApiResponsesListModel>(result);
        }

        /// <summary>
        /// Removes an API response from the database.
        /// </summary>
        /// <param name="id">The unique identifier of the API response to be removed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Remove(int id)
        {
            // Retrieves entity by ID from the repository
            var result = _uow.ApiResponsesRepository.GetById(id);
            // Removes the entity from the repository
            _uow.ApiResponsesRepository.Remove(result);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Updates an existing API response in the database.
        /// </summary>
        /// <param name="model">The updated API response data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(ApiResponsesCreateUpdateModel model)
        {
            // Retrieves existing entity by ID from the repository
            var apiResponce = _uow.ApiResponsesRepository.GetById(model.ResponceId);
            // Maps the updated model to entity using AutoMapper
            var entity = _mapper.Map<ApiResponse>(apiResponce);
            // Sets the update date time to current time
            entity.UpdatedByDateTime = DateTime.Now;
            // Updates the entity in the repository
            _uow.ApiResponsesRepository.Update(entity);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }
    }
}
