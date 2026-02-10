using MapsterMapper;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Provides functionality to manage application settings.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AppSettingService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class AppSettingService(IUnitOfWork uow, IMapper mapper) : IAppSettingService
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
        /// Retrieves all application settings.
        /// </summary>
        /// <returns>A list of application settings.</returns>
        public List<AppSettingModel> GetAll()
        {
            // Retrieves all application settings from the repository
            var categories = _uow.AppSettingRepository.GetAll();
            // Maps the entities to models using AutoMapper and returns the result
            return _mapper.Map<List<AppSettingModel>>(categories);
        }

        /// <summary>
        /// Retrieves an application setting by its ID.
        /// </summary>
        /// <param name="id">The ID of the application setting.</param>
        /// <returns>The application setting model if found; otherwise, null.</returns>
        public AppSettingModel GetById(int id)
        {
            // Retrieves the application setting entity by ID from the repository
            var appSetting = _uow.AppSettingRepository.GetById(id);
            // Maps the entity to a model using AutoMapper and returns the result
            return _mapper.Map<AppSettingModel>(appSetting);
        }

        /// <summary>
        /// Adds a new application setting.
        /// </summary>
        /// <param name="appSetting">The application setting model to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(AppSettingModel appSetting)
        {
            // Maps the model to an entity using AutoMapper
            _uow.AppSettingRepository.Add(_mapper.Map<AppSetting>(appSetting));
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Updates an existing application setting.
        /// </summary>
        /// <param name="appSetting">The updated application setting model.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(AppSettingModel appSetting)
        {
            // Retrieves the existing entity by ID from the repository
            var item = _uow.AppSettingRepository.GetById(appSetting.AppSettingId);
            // Updates the entity with values from the model using AutoMapper
            _uow.AppSettingRepository.Update(_mapper.Map<AppSettingModel, AppSetting>(appSetting, item));
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Removes an application setting by its ID.
        /// </summary>
        /// <param name="id">The ID of the application setting to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Remove(int id)
        {
            // Retrieves the entity by ID from the repository
            var item = _uow.AppSettingRepository.GetById(id);
            // Removes the entity from the repository
            _uow.AppSettingRepository.Remove(item);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Removes multiple application settings by their IDs.
        /// </summary>
        /// <param name="ids">A list of IDs of application settings to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RemoveMultiple(List<int> ids)
        {
            // Iterates through each ID in the list
            foreach (var id in ids)
            {
                // Retrieves the entity by ID from the repository
                var item = _uow.AppSettingRepository.GetById(id);
                // Checks if the entity exists
                if (item != null)
                {
                    // Removes the entity from the repository
                    _uow.AppSettingRepository.Remove(item);
                }
            }
            // Commits all changes to the database
            await _uow.CompleteAsync();
        }
    }
}
