using AutoMapper;
using MEligibilityPlatform.Application.Services.Inteface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Provides functionality to manage user statuses.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UserStatusService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class UserStatusService(IUnitOfWork uow, IMapper mapper) : IUserStatusService
    {
        // The unit of work instance for database operations.
        private readonly IUnitOfWork _uow = uow;

        // The AutoMapper instance for object mapping.
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Adds a new user status to the database.
        /// </summary>
        /// <param name="userStatusModel">The UserStatusModel containing the data to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(UserStatusAddModel userStatusModel)
        {
            // Retrieves the last user status to determine the next ID.
            var lastUserStatus = await _uow.UserStatusRepository.GetLastUserStatus();
            // Calculates the new ID (1 if no existing statuses, otherwise increment).
            int newId = lastUserStatus == null ? 1 : lastUserStatus.StatusId + 1;
            // Assigns the calculated ID to the model.
            userStatusModel.StatusId = newId;
            // Sets the update timestamp to current UTC time.
            userStatusModel.UpdatedByDateTime = DateTime.UtcNow;
            // Maps the model to entity and adds to repository.
            _uow.UserStatusRepository.Add(_mapper.Map<UserStatus>(userStatusModel));
            // Commits the changes to the database.
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Gets all user statuses.
        /// </summary>
        /// <returns>A list of UserStatusModel representing all user statuses.</returns>
        public List<UserStatusModel> GetAll()
        {
            // Retrieves all user statuses from repository.
            var userStatuses = _uow.UserStatusRepository.GetAll();
            // Maps the entities to models and returns.
            return _mapper.Map<List<UserStatusModel>>(userStatuses);
        }

        /// <summary>
        /// Gets a user status by its ID.
        /// </summary>
        /// <param name="id">The user status ID to retrieve.</param>
        /// <returns>The UserStatusModel for the specified ID.</returns>
        public UserStatusModel GetById(int id)
        {
            // Retrieves user status entity by ID.
            var userStatus = _uow.UserStatusRepository.GetById(id);
            // Maps the entity to model and returns.
            return _mapper.Map<UserStatusModel>(userStatus);
        }

        /// <summary>
        /// Removes a user status by its ID.
        /// </summary>
        /// <param name="id">The user status ID to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Remove(int id)
        {
            // Retrieves the user status entity by ID.
            var item = _uow.UserStatusRepository.GetById(id);
            // Removes the entity from repository.
            _uow.UserStatusRepository.Remove(item);
            // Commits the changes to the database.
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Updates an existing user status.
        /// </summary>
        /// <param name="userStatusModel">The UserStatusModel containing updated data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(UserStatusAddModel userStatusModel)
        {
            // Retrieves the existing user status entity by ID.
            var item = _uow.UserStatusRepository.GetById(userStatusModel.StatusId);
            // Sets the update timestamp to current UTC time.
            userStatusModel.UpdatedByDateTime = DateTime.UtcNow;
            // Maps updated model to existing entity and updates in repository.
            _uow.UserStatusRepository.Update(_mapper.Map<UserStatusAddModel, UserStatus>(userStatusModel, item));
            // Commits the changes to the database.
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes multiple user statuses by their IDs.
        /// </summary>
        /// <param name="ids">A list of user status IDs to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task MultipleDelete(List<int> ids)
        {
            //throw new NotImplementedException();

            // Validates each ID before deletion.
            foreach (var id in ids)
            {
                // Checks if the user status exists in the database.
                var hasvalue = await _uow.UserStatusRepository.Query().AnyAsync(item => item.StatusId == id);
                // Throws exception if ID doesn't exist.
                if (hasvalue == false)
                {
                    // Provides detailed error message about invalid IDs.
                    throw new Exception($"these  id's: {id} is not present. please provide valid id. ");
                }
                //var manageditem = await _uow.ListItemRepository.Query().AnyAsync(entity => entity.ListId == id);

                //if (manageditem)
                //{
                //    throw new Exception($"Cannot delete the entity {id} because it{id} has dependencies.");
                //}
            }

            // Deletes each validated user status.
            foreach (var id in ids)
            {
                // Retrieves the user status entity by ID.
                var manageitem = _uow.UserStatusRepository.GetById(id);
                // Checks if entity exists before removal.
                if (manageitem != null)
                {
                    // Removes the entity from repository.
                    _uow.UserStatusRepository.Remove(manageitem);
                }
            }
            // Commits all deletion changes to the database.
            await _uow.CompleteAsync();
        }
    }
}
