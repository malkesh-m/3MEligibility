using AutoMapper;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Application.UnitOfWork;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing settings.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="SettingService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class SettingService(IUnitOfWork uow, IMapper mapper) : ISettingService
    {
        /// The unit of work instance for database operations.
        private readonly IUnitOfWork _uow = uow;

        /// The AutoMapper instance for object mapping.
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Updates a setting for a specific entity.
        /// </summary>
        /// <param name="model">The SettingModel containing updated data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(SettingModel model)
        {
            // Queries for existing setting by entity ID
            var Item = _uow.SettingRepository.Query().FirstOrDefault(w => w.TenantId == model.EntityId);
            if (Item == null)
            {
                // Creates new setting if none exists for the entity
                var newSetting = new Setting
                {
                    TenantId = model.EntityId,
                    IsMakerCheckerEnable = model.IsMakerCheckerEnable
                };

                // Adds the new setting to repository
                _uow.SettingRepository.Add(newSetting);
            }
            else
            {
                // Updates existing setting with new values
                Item.IsMakerCheckerEnable = model.IsMakerCheckerEnable;
                // Updates the setting in repository
                _uow.SettingRepository.Update(Item);
            }
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        //public SettingModel Get(int id)
        //{
        //    var setting = _uow.SettingRepository.GetById(1);
        //    return new SettingModel { IsMakerCheckerEnable = setting.IsMakerCheckerEnable };
        //}

        /// <summary>
        /// Gets a setting by entity ID and setting ID.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="id">The setting ID.</param>
        /// <returns>The SettingModel for the specified entity and setting ID.</returns>
        public SettingModel GetById(int tenantId, int id)
        {
            // Retrieves setting by both entity ID and setting ID
            var setting = _uow.SettingRepository.Query().First(w => w.SettingId == id && w.TenantId == tenantId);
            // Returns mapped setting model
            return new SettingModel { IsMakerCheckerEnable = setting.IsMakerCheckerEnable };
        }

        /// <summary>
        /// Gets a setting by entity ID.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>A task representing the asynchronous operation, with the SettingModel for the specified entity.</returns>
        public async Task<SettingModel> GetbyEntityId(int tenantId)
        {
            // Queries for setting by entity ID
            var setting = _uow.SettingRepository.Query().FirstOrDefault(w => w.TenantId == tenantId);
            if (setting == null)
            {
                // Creates default setting if none exists
                var settings = new SettingModel { IsMakerCheckerEnable = false, EntityId = tenantId };
                // Persists the default setting
                await Update(settings);
                // Returns the default setting
                return settings;
            }
            // Returns existing setting mapped to model
            return new SettingModel { IsMakerCheckerEnable = setting.IsMakerCheckerEnable };
        }

        /// <summary>
        /// Adds a new setting to the database.
        /// </summary>
        /// <param name="model">The SettingModel containing the data to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(SettingModel model)
        {
            // Maps the incoming model to Setting entity and adds to repository
            _uow.SettingRepository.Add(_mapper.Map<Setting>(model));
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }
    }
}
