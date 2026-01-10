using AutoMapper;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Application.UnitOfWork;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing screens.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ScreenService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class ScreenService(IUnitOfWork uow, IMapper mapper) : IScreenService
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
        /// Adds a new screen to the database.
        /// </summary>
        /// <param name="screenModel">The ScreenModel containing the data to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(ScreenModel screenModel)
        {
            // Sets the update timestamp to current UTC time
            screenModel.UpdatedByDateTime = DateTime.UtcNow;
            // Maps the incoming model to Screen entity and adds to repository
            _uow.ScreenRepository.Add(_mapper.Map<Screen>(screenModel));
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Gets all screens.
        /// </summary>
        /// <returns>A list of ScreenModel representing all screens.</returns>
        public List<ScreenModel> GetAll()
        {
            // Retrieves all screens from the repository
            var screens = _uow.ScreenRepository.GetAll();
            // Maps the screens to ScreenModel objects
            return _mapper.Map<List<ScreenModel>>(screens);
        }

        /// <summary>
        /// Gets a screen by its ID.
        /// </summary>
        /// <param name="id">The screen ID to retrieve.</param>
        /// <returns>The ScreenModel for the specified ID.</returns>
        public ScreenModel GetById(int id)
        {
            // Retrieves the specific screen by ID
            var screen = _uow.ScreenRepository.GetById(id);
            // Maps the screen to ScreenModel object
            return _mapper.Map<ScreenModel>(screen);
        }

        /// <summary>
        /// Removes a screen by its ID.
        /// </summary>
        /// <param name="id">The screen ID to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Remove(int id)
        {
            // Retrieves the screen by ID
            var item = _uow.ScreenRepository.GetById(id);
            // Removes the screen from the repository
            _uow.ScreenRepository.Remove(item);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Updates an existing screen.
        /// </summary>
        /// <param name="screenModel">The ScreenModel containing updated data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(ScreenModel screenModel)
        {
            // Retrieves the existing screen by ID
            var item = _uow.ScreenRepository.GetById(screenModel.ScreenId);
            // Sets the update timestamp to current UTC time
            screenModel.UpdatedByDateTime = DateTime.UtcNow;
            // Updates the screen with mapped data from the model
            _uow.ScreenRepository.Update(_mapper.Map<ScreenModel, Screen>(screenModel, item));
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes multiple screens by their IDs.
        /// </summary>
        /// <param name="ids">A list of screen IDs to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task MultipleDelete(List<int> ids)
        {
            // Validates that all provided IDs exist
            foreach (var id in ids)
            {
                // Checks if the screen exists in the database
                var hasvalue = await _uow.ScreenRepository.Query().AnyAsync(item => item.ScreenId == id);
                // Throws exception if any ID is not found
                if (hasvalue == false)
                {
                    throw new Exception($"these  id's: {id} is not present. please provide valid id. ");
                }
            }

            // Deletes all validated screens
            foreach (var id in ids)
            {
                // Retrieves each screen by ID
                var manageitem = _uow.ScreenRepository.GetById(id);
                // Removes the screen if found
                if (manageitem != null)
                {
                    _uow.ScreenRepository.Remove(manageitem);
                }
            }

            // Commits all deletion changes to the database
            await _uow.CompleteAsync();
        }
    }
}
