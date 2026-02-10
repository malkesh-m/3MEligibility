using System.Net.Http.Json;
using System.Security.Claims;
using MapsterMapper;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Application.Helper;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Crypto;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing user-related operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UserService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="emailService">The email service instance.</param>
    /// <param name="tokenService">The token service instance.</param>
    public class UserService(IUnitOfWork uow, IMapper mapper, IConfiguration configuration, IEmailService emailService, ITokenService tokenService, ILdapService ldapService, IMemoryCache cache, IHttpClientFactory httpClientFactory) : IUserService
    {
        // The unit of work instance for database operations.
        private readonly IUnitOfWork _uow = uow;
        private readonly IMemoryCache _cache = cache;
        // The AutoMapper instance for object mapping.
        private readonly IMapper _mapper = mapper;

        // The configuration instance for application settings.
        private readonly IConfiguration _configuration = configuration;

        // The email service instance for sending emails.
        private readonly IEmailService _emailService = emailService;

        // The token service instance for token operations.
        private readonly ITokenService _tokenService = tokenService;
        private readonly ILdapService _ldaService = ldapService;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        // The token expiration time span (1 hour).
        private readonly TimeSpan _tokenExpiration = TimeSpan.FromHours(1);
        /// <summary>
        /// Adds a new user to the database, including optional image upload.
        /// </summary>
        /// <param name="fileData">The uploaded image file.</param>
        /// <param name="userModel">The user model containing user data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        //public async Task Add(IFormFile? fileData, UserAddModel userModel, string userName)
        //{
        //    // Queries the database to check if a user with the same email or login ID already exists for the given entity.
        //    var item = _uow.UserRepository.Query().FirstOrDefault(x => x.EntityId == userModel.EntityId && x.Email == userModel.Email || x.LoginId == userModel.LoginId);

        //    // Checks if a duplicate user was found.
        //    if (item is not null)
        //    {
        //        // Throws an exception indicating which field (email or login ID) already exists.
        //        throw new Exception($"{(item.Email == userModel.Email ? nameof(item.Email) : nameof(item.LoginId))} already exist please provide unique value.");
        //    }

        //    // Creates a new User entity instance.
        //    var user = new User()
        //    {
        //        // Sets the user name from the model.
        //        UserName = userModel.UserName,
        //        // Sets the email from the model.
        //        Email = userModel.Email,
        //        // Sets the login ID from the model.
        //        LoginId = userModel.LoginId,
        //        // Initializes password as empty (commented out encryption alternative).
        //        UserPassword = string.Empty, // EncryptDecrypt.EncryptString(userModel.UserPassword, _configuration["EncryptionSettings:Key"]),
        //        // Sets the phone number from the model.
        //        Phone = userModel.Phone,
        //        // Sets the entity ID from the model.
        //        EntityId = userModel.EntityId,
        //        // Sets the creation date to today's UTC date.
        //        CreationDate = DateOnly.FromDateTime(DateTime.UtcNow),
        //        // Sets the initial status ID to 2 (typically "Active" or similar).
        //        StatusId = userModel.StatusId,
        //        CreatedBy = userName,
        //        UpdatedBy = userName,
        //        LastPasswordUpdate = DateTime.Now
        //    };
        //    // Creates a new PasswordHasher instance for the User entity.
        //    PasswordHasher<User> _passwordHasher = new();
        //    // Hashes the plain text password from the model and assigns it to the user.
        //    user.UserPassword = _passwordHasher.HashPassword(user, userModel.UserPassword);

        //    // Checks if a file was uploaded.
        //    if (fileData != null)
        //    {
        //        // Creates an instance of the image upload helper.
        //        var imageUploadHelper = new ImageUploadHelper();
        //        // Processes the uploaded image file and gets the byte array and MIME type.
        //        var (imageBytes, mimeType) = await ImageUploadHelper.ProcessImageUpload(fileData);
        //        // Assigns the image bytes to the user's picture property.
        //        user.UserPicture = imageBytes;
        //        // Assigns the MIME type to the user's MIME type property.
        //        user.MimeType = mimeType;
        //    }
        //    // Sets the last updated timestamp to current UTC time.
        //    user.UpdatedByDateTime = DateTime.UtcNow;
        //    // Adds the new user entity to the repository.
        //    _uow.UserRepository.Add(user);
        //    // Saves all changes to the database.
        //    await _uow.CompleteAsync();
        //    // Outputs the length of the user's picture to the console for debugging.
        //    Console.WriteLine($"userpicture length{user.UserPicture?.Length}");
        //}

        /// <summary>
        /// Gets all users for a specific entity.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <returns>A list of UserGetModel representing all users for the entity.</returns>
        public async Task<ApiResponse<List<UserGetModel>>> GetAll(int tenantId)
        {
            var client = _httpClientFactory.CreateClient("MIdentityAPI");
            var response = await client.GetAsync($"api/v1/Users/GetAllByTenantId/{tenantId}");
            // Executes the query and returns the results as a list.
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadFromJsonAsync<ApiResponse<List<UserGetModel>>>();
            return data ?? new ApiResponse<List<UserGetModel>>();
        }

        ///// <summary>
        ///// Gets a user by entity ID and user ID.
        ///// </summary>
        ///// <param name="entityId">The entity ID.</param>
        ///// <param name="id">The user ID.</param>
        ///// <returns>The UserGetModel for the specified entity and user ID.</returns>
        //public UserGetModel GetById(int entityId, int id)
        //{
        //    // Queries the database for a user with the specified ID and entity ID.
        //    var user = _uow.UserRepository.Query().FirstOrDefault(f => f.UserId == id && f.EntityId == entityId);
        //    // Checks if the user was not found or is deactivated.
        //    if (user == null || user.StatusId == 3)
        //    {
        //        // Throws an exception if the user is invalid.
        //        throw new Exception("Invalid UserId or UserIdAlready Deactivated");
        //    }

        //    // Uses AutoMapper to map the User entity to a UserGetModel.
        //    var item = _mapper.Map<UserGetModel>(user);
        //    // Performs a subquery to get the groups the user belongs to.
        //    item.Groups = [.. (from ug in _uow.UserGroupRepository.Query()
        //                       // Joins user groups with security groups.
        //                   join sg in _uow.SecurityGroupRepository.Query() on ug.GroupId equals sg.GroupId
        //                   // Filters for the current user.
        //                   where ug.UserId == item.UserId
        //                   // Projects into GroupModel.
        //                   select new GroupModel
        //                   {
        //                       // Maps group ID.
        //                       GroupId = ug.GroupId,
        //                       // Maps group name.
        //                       GroupName = sg.GroupName ?? ""
        //                   })];
        //    // Returns the populated UserGetModel.
        //    return item;
        //}

        ///// <summary>
        ///// Gets the group list for a user by entity ID and user ID.
        ///// </summary>
        ///// <param name="entityId">The entity ID.</param>
        ///// <param name="userId">The user ID.</param>
        ///// <returns>A list of GroupModel for the specified user.</returns>
        //public List<GroupModel> GetGroupList(int entityId, int userId)
        //{
        //    // Queries the database for groups associated with the user.
        //    List<GroupModel> item = [.. (from ug in _uow.UserGroupRepository.Query()
        //                                 // Joins user groups with security groups.
        //                             join sg in _uow.SecurityGroupRepository.Query() on ug.GroupId equals sg.GroupId
        //                             // Filters for the specified user ID.
        //                             where ug.UserId == userId
        //                             // Projects into GroupModel.
        //                             select new GroupModel
        //                             {
        //                                 // Maps group ID.
        //                                 GroupId = ug.GroupId,
        //                                 // Maps group name.
        //                                 GroupName = sg.GroupName ?? ""
        //                             })];
        //    // Returns the list of groups.
        //    return item; //TODO: add EntityId check condition
        //}

        ///// <summary>
        ///// Removes (deactivates) a user by entity ID and user ID.
        ///// </summary>
        ///// <param name="entityId">The entity ID.</param>
        ///// <param name="id">The user ID.</param>
        ///// <returns>A task representing the asynchronous operation.</returns>
        //public async Task Remove(int entityId, int id)
        //{
        //    // Retrieves the user entity by ID and entity ID.
        //    var item = _uow.UserRepository.Query().First(f => f.UserId == id && f.EntityId == entityId);
        //    // Sets the user's status to 3 (Deactivated).
        //    item.StatusId = 3;
        //    // Marks the entity as modified in the repository.
        //    _uow.UserRepository.Update(item);
        //    // Saves the changes to the database.
        //    await _uow.CompleteAsync();
        //}
        //public async Task RemoveUsers(int entityId, List<int> ids)
        //{
        //    foreach (var id in ids)
        //    {
        //        // Retrieves the factor by ID and entity ID
        //        var item = _uow.UserRepository.Query().FirstOrDefault(f => f.UserId == id);
        //        // Checks if the item exists
        //        if (item != null)
        //        {
        //            // Removes the item from the repository
        //            item.StatusId = 3;
        //            _uow.UserRepository.Update(item);
        //        }
        //    }
        //    // Saves the changes to the database.
        //    await _uow.CompleteAsync();
        //}
        //public async Task<string> ReActivateUser(ReActivationModel user)
        //{
        //    // Retrieves the user entity by ID and entity ID.
        //    var item = _uow.UserRepository.Query().First(f => f.UserId == user.UserId);
        //    if (item.Issuspended == false)
        //    {
        //        throw new Exception("User already activate");
        //    }
        //    // Sets the user's status to 3 (Deactivated).
        //    item.Issuspended = false;
        //    // Marks the entity as modified in the repository.
        //    _uow.UserRepository.Update(item);
        //    // Saves the changes to the database.
        //    await _uow.CompleteAsync();
        //    return "User Activate Sucessfully";
        //}

        ////public async Task RemoveId(int entityId, int id)
        ////{
        ////    var item = _uow.UserRepository.Query().First(f => f.UserId == id && f.EntityId == entityId);
        ////    item.StatusId = 3;
        ////    _uow.UserRepository.Update(item);
        ////    await _uow.CompleteAsync();
        ////}

        ///// <summary>
        ///// Updates a user's information, including optional image upload.
        ///// </summary>
        ///// <param name="fileData">The uploaded image file.</param>
        ///// <param name="userModel">The user edit model containing updated data.</param>
        ///// <returns>A task representing the asynchronous operation.</returns>
        //public async Task Update(IFormFile? fileData, UserEditModel userModel, string updatedBy)
        //{
        //    // Creates an instance of the image upload helper.
        //    var imageUploadHelper = new ImageUploadHelper();
        //    // Creates a new PasswordHasher instance (not used in this method but initialized).
        //    PasswordHasher<User> _passwordHasher = new();
        //    // Retrieves the existing user entity from the database.
        //    var user = _uow.UserRepository.Query().FirstOrDefault(f => f.UserId == userModel.UserId);
        //    // Checks if a new image file was provided.
        //    if (fileData != null)
        //    {
        //        // Processes the uploaded image file.
        //        var (imageBytes, mimeType) = await ImageUploadHelper.ProcessImageUpload(fileData);
        //        // Validates that the image was processed successfully.
        //        if (imageBytes == null || imageBytes.Length == 0)
        //        {
        //            // Throws an exception if image processing failed.
        //            throw new Exception("Image processing failed or returned empty data.");
        //        }
        //        // Updates the user entity and model with the new image bytes.
        //        user!.UserPicture = userModel.UserPicture = imageBytes;
        //        // Updates the MIME type.
        //        user.MimeType = mimeType;
        //    }

        //    //user.Email = userModel.Email == null ? user.Email : userModel.Email;
        //    //user.Phone = userModel.Phone == null ? user.Phone : userModel.Phone;

        //    //user.UserPassword = userModel.UserPassword = _passwordHasher.HashPassword(user, userModel.UserPassword);

        //    // Updates the status ID, defaulting to 2 if null.
        //    user!.StatusId = userModel.StatusId == null ? 2 : userModel.StatusId;
        //    // Sets the update timestamp.
        //    user.UpdatedByDateTime = DateTime.UtcNow;
        //    user.Phone = userModel.Phone ?? "";
        //    user.UpdatedBy = updatedBy;

        //    // Uses AutoMapper to apply updates from the model to the entity and marks it as updated.
        //    _uow.UserRepository.Update(_mapper.Map<UserEditModel, User>(userModel, user));

        //    // Saves changes to the database.
        //    await _uow.CompleteAsync();
        //    // Logs the image length for debugging.
        //    Console.WriteLine($"userpicture length{user.UserPicture?.Length}");
        //}

        ///// <summary>
        ///// Updates a user's profile picture.
        ///// </summary>
        ///// <param name="entityId">The entity ID.</param>
        ///// <param name="fileData">The uploaded image file.</param>
        ///// <param name="model">The user picture model.</param>
        ///// <returns>A task representing the asynchronous operation, with a boolean indicating success.</returns>
        //public async Task<bool> UpdatePic(int entityId, IFormFile? fileData, UserPictureModel model)
        //{
        //    // Creates an instance of the image upload helper.
        //    //var imageUploadHelper = new ImageUploadHelper();
        //    // Retrieves the user entity by ID.
        //    var user = _uow.UserRepository.GetById(model.UserId);
        //    // Checks if a file was provided.
        //    if (fileData != null)
        //    {
        //        // Processes the uploaded image.
        //        var (imageBytes, mimeType) = await ImageUploadHelper.ProcessImageUpload(fileData);
        //        // Validates the processed image.
        //        if (imageBytes == null || imageBytes.Length == 0)
        //        {
        //            throw new Exception("Image processing failed or returned empty data.");
        //        }
        //        // Updates the user entity and model with the new image.
        //        user.UserPicture = model.UserPicture = imageBytes;
        //        // Updates the MIME type.
        //        user.MimeType = mimeType;
        //    }

        //    //user.Email = userModel.Email == null ? user.Email : userModel.Email;
        //    //user.Phone = userModel.Phone == null ? user.Phone : userModel.Phone;

        //    //user.UserPassword = userModel.UserPassword = _passwordHasher.HashPassword(user, userModel.UserPassword);

        //    //user.StatusId = userModel.StatusId == null ? 2 : userModel.StatusId;

        //    // Ensures the entity ID matches the parameter.
        //    user.EntityId = entityId;
        //    // Applies updates from the model to the entity and marks it as updated.
        //    _uow.UserRepository.Update(_mapper.Map<UserPictureModel, User>(model, user));

        //    // Saves changes to the database.
        //    await _uow.CompleteAsync();
        //    // Logs the image length for debugging.
        //    Console.WriteLine($"userpicture length{user.UserPicture?.Length}");
        //    // Returns true indicating success.
        //    return true;
        //}

        //// Windows Ad authentication code 

        ///// <summary>
        ///// Authenticates a user by email and password.
        ///// </summary>
        ///// <param name="email">The user's email or login ID.</param>
        ///// <param name="password">The user's password.</param>
        ///// <returns>A task representing the asynchronous operation, with the authenticated User object.</returns>
        ////public async Task<User?> AuthenticateUser(string email, string password)
        ////{
        ////    var user = await _uow.UserRepository.Query()
        ////               .FirstOrDefaultAsync(u => u.LoginId == email)
        ////               ?? throw new Exception("Invalid user.");

        ////    // NEW: LDAP Validation
        ////    var (IsAuthenticated, UserAttributes) = await _ldaService.Authenticate(email, password);

        ////    if (!IsAuthenticated)
        ////    {
        ////        user.NoOfTrials++;

        ////        if (user.NoOfTrials > 2)
        ////        {
        ////            user.Issuspended = true;
        ////            user.SuspentionDate = DateTime.Now;
        ////        }

        ////        _uow.UserRepository.Update(user);
        ////        await _uow.CompleteAsync();

        ////        throw new Exception("Invalid username or password.");
        ////    }

        ////    // Reset attempts
        ////    user.NoOfTrials = 0;

        ////    // check if suspended
        ////    if (user.Issuspended == true && user.SuspentionDate.HasValue)
        ////    {
        ////        var suspensionDurationInHours = _configuration.GetValue<int>("AppSettings:SuspensionDurationInHours");
        ////        var suspensionEndTime = user.SuspentionDate.Value.AddHours(suspensionDurationInHours);

        ////        if (DateTime.Now < suspensionEndTime)
        ////            throw new Exception("Your account is suspended. Please try again later.");

        ////        user.Issuspended = false;
        ////        user.SuspentionDate = null;
        ////    }

        ////    // Load group & role
        ////    var userGroup = await _uow.UserGroupRepository.Query()
        ////                    .FirstOrDefaultAsync(u => u.UserId == user.UserId) ?? throw new Exception("User has no role assigned.");
        ////    user.SecurityGroup = await _uow.SecurityGroupRepository.Query()
        ////                          .FirstOrDefaultAsync(u => u.GroupId == userGroup.GroupId);

        ////    user.StatusId = 1;
        ////    _uow.UserRepository.Update(user);
        ////    await _uow.CompleteAsync();

        ////    return user;
        ////}



        ////Normal Authentication

        //public async Task<User?> AuthenticateUser(string email, string password)
        //{
        //    // Queries the database for a user by login ID (email parameter is used as login ID).
        //    var user = await _uow.UserRepository.Query().FirstOrDefaultAsync(u => u.LoginId == email) ?? throw new Exception("Invalid username or password");

        //    // Retrieves the suspension duration from app settings.
        //    var suspensionDurationInHours = _configuration.GetValue<int>("AppSettings:SuspensionDurationInHours");
        //    var LoginPasswordExpiryDays = _configuration.GetValue<int>("AppSettings:LoginPasswordExpiryDays");

        //    //var encryptedPassword = EncryptDecrypt.EncryptString(password, _configuration["EncryptionSettings:Key"]);

        //    // Creates a password hasher instance.
        //    PasswordHasher<User> _passwordHasher = new();
        //    // Verifies the provided password against the stored hash.
        //    if (_passwordHasher.VerifyHashedPassword(user, user.UserPassword, password) != PasswordVerificationResult.Success)
        //    {
        //        // Increments the failed login attempt counter.
        //        user.NoOfTrials++;
        //        // Checks if the attempt threshold (3) is exceeded.
        //        if (user.NoOfTrials > 2)
        //        {
        //            // Suspends the user account.
        //            user.Issuspended = true;
        //            // Records the suspension time.
        //            user.SuspentionDate = DateTime.Now;
        //        }

        //        // Updates the user entity in the repository.
        //        _uow.UserRepository.Update(user);
        //        // Saves the changes.
        //        await _uow.CompleteAsync();

        //        // Returns null indicating authentication failed.
        //        return null;
        //    }

        //    // Checks if the user is currently suspended.
        //    if (user.Issuspended == true && user.SuspentionDate.HasValue)
        //    {
        //        // Calculates when the suspension should end.
        //        var suspensionEndTime = user.SuspentionDate.Value.AddHours(suspensionDurationInHours);

        //        // Checks if the suspension period has elapsed.
        //        if (DateTime.Now >= suspensionEndTime)
        //        {
        //            // Removes the suspension.
        //            user.Issuspended = false;
        //            // Clears the suspension date.
        //            user.SuspentionDate = null;
        //            // Updates the user entity.
        //            _uow.UserRepository.Update(user);
        //            // Saves the changes.
        //            await _uow.CompleteAsync();

        //        }
        //        else
        //        {
        //            // Throws an exception if still suspended.
        //            throw new Exception("Your account is suspended due to too many failed login attempts. Please try again later.");
        //        }
        //    }

        //    // Resets the failed login counter on successful authentication.
        //    user.NoOfTrials = 0;

        //    // Retrieves the user's group membership.
        //    var userGroup = await _uow.UserGroupRepository.Query().FirstOrDefaultAsync(u => u.UserId == user.UserId);
        //    // Checks if the user has a group assigned.
        //    if (userGroup != null)
        //    {
        //        // Retrieves the security group details.
        //        user.SecurityGroup = await _uow.SecurityGroupRepository.Query().FirstOrDefaultAsync(u => u.GroupId == userGroup.GroupId);
        //        // Sets the user status to 1 (typically "LoggedIn" or "Active").
        //        user.StatusId = 1;
        //        user.LastLoginDate = DateTime.Now;
        //        if (user.LastPasswordUpdate == DateTime.MinValue)
        //        {
        //            user.ForcePasswordChange = true;
        //            return user;
        //        }

        //        var passwordAge = (DateTime.Now - user.LastPasswordUpdate).TotalDays;
        //        if (passwordAge > LoginPasswordExpiryDays)
        //        {
        //            user.ForcePasswordChange = true;
        //            return user;
        //        }
        //        else
        //        {
        //            user.ForcePasswordChange = false;
        //        }
        //        // Updates the user entity.
        //        _uow.UserRepository.Update(user);
        //        // Saves the changes.
        //        await _uow.CompleteAsync();

        //        // Returns the authenticated user.
        //        return user;
        //    }
        //    else
        //        // Throws an exception if no group is assigned.
        //        throw new Exception("User has no role assigned.");

        //    //            return await _uow.UserRepository.AuthenticateUser(email, password, suspensionDurationInHours);
        //}



        ///// <summary>
        ///// Suspends a user by entity ID and user ID.
        ///// </summary>
        ///// <param name="entityId">The entity ID.</param>
        ///// <param name="userId">The user ID.</param>
        ///// <returns>A task representing the asynchronous operation.</returns>
        //public async Task SuspendUser(int entityId, int userId)
        //{
        //    // Retrieves the user entity.
        //    var user = _uow.UserRepository.Query().First(f => f.UserId == userId && f.EntityId == entityId);
        //    // Sets the suspended flag.
        //    user.Issuspended = true;
        //    // Records the suspension time.
        //    user.SuspentionDate = DateTime.Now;
        //    // Marks the entity as updated.
        //    _uow.UserRepository.Update(user);
        //    // Saves the changes.
        //    await _uow.CompleteAsync();
        //}

        ///// <summary>
        ///// Verifies a user's password.
        ///// </summary>
        ///// <param name="currentUserId">The current user ID.</param>
        ///// <param name="currentPassword">The current password.</param>
        ///// <returns>A task representing the asynchronous operation, with a boolean indicating if the password is correct.</returns>
        //public async Task<bool> VerifyPassword(int currentUserId, string currentPassword)
        //{
        //    // Retrieves the user entity.
        //    var user = await _uow.UserRepository.Query().FirstOrDefaultAsync(u => u.UserId == currentUserId) ?? throw new Exception("Invalid user.");
        //    // Creates a password hasher instance.
        //    PasswordHasher<User> _passwordHasher = new();
        //    // Returns true if password verification is successful.
        //    return _passwordHasher.VerifyHashedPassword(user, user.UserPassword, currentPassword) == PasswordVerificationResult.Success;
        //}

        ///// <summary>
        ///// Checks if a user has permission for a specific controller and menu URL.
        ///// </summary>
        ///// <param name="user">The ClaimsPrincipal representing the user.</param>
        ///// <param name="controllerName">The controller name.</param>
        ///// <param name="menuUrl">The menu URL.</param>
        ///// <returns>A task representing the asynchronous operation, with a boolean indicating permission.</returns>
        //public async Task<bool> UserHasPermission(ClaimsPrincipal user, string controllerName, string menuUrl)
        //{
        //    // Extracts the user's role from claims.
        //    var userRoles = GetUserRoles(user);
        //    // Checks if role was found.
        //    if (userRoles == null)
        //    {
        //        // Returns false if no role.
        //        return false;
        //    }

        //    // Retrieves the security group based on the role name.
        //    var securityGroup = await _uow.SecurityGroupRepository.GetSecurityGroup(userRoles);
        //    // Checks if security group was found.
        //    if (securityGroup == null)
        //    {
        //        // Returns false if no group.
        //        return false;
        //    }

        //    // Retrieves screen IDs associated with the controller name.
        //    var screens = await _uow.ScreenRepository.GetScreen(controllerName);
        //    // Checks if screens were found.
        //    if (screens == null)
        //    {
        //        // Returns false if no screens.
        //        return false;
        //    }

        //    // Retrieves role IDs associated with the security group.
        //    var groupRoles = await _uow.GroupRoleRepository.GetGroupRoles(securityGroup.GroupId);
        //    // Checks if any roles exist for the group.
        //    if (groupRoles == null || groupRoles.Count == 0)
        //    {
        //        // Returns false if no roles.
        //        return false;
        //    }

        //    // Checks if the user has permission for the specific menu URL.
        //    return await HasMenuPermissions(groupRoles, menuUrl, screens);
        //    //return await _uow.UserRepository.UserHasPermission(user, controllerName, menuUrl);
        //}

        ///// <summary>
        ///// Changes a user's password.
        ///// </summary>
        ///// <param name="userId">The user ID.</param>
        ///// <param name="currentPassword">The current password.</param>
        ///// <param name="newPassword">The new password.</param>
        ///// <returns>A task representing the asynchronous operation, with a boolean indicating success.</returns>
        //public async Task<bool> ChangePassword(int userId, string currentPassword, string newPassword)
        //{
        //    // Retrieves the user entity by ID.
        //    var user = _uow.UserRepository.GetById(userId);
        //    // Creates a password hasher instance.
        //    PasswordHasher<User> _passwordHasher = new();
        //    // Hashes the new password.
        //    user.UserPassword = _passwordHasher.HashPassword(user, newPassword);
        //    user.ForcePasswordChange = false;
        //    user.LastPasswordUpdate = DateTime.Now;
        //    // Marks the entity as updated.
        //    _uow.UserRepository.Update(user);
        //    // Saves the changes.
        //    await _uow.CompleteAsync();

        //    // Returns true indicating success.
        //    return true;
        //}

        ///// <summary>
        ///// Requests a password reset for a user by email.
        ///// </summary>
        ///// <param name="email">The user's email.</param>
        ///// <param name="resetLink">The reset link to send.</param>
        ///// <returns>A task representing the asynchronous operation, with a boolean indicating success.</returns>
        //public async Task<bool> RequestPasswordReset(string email, string resetLink)
        //{
        //    // Retrieves the user by email address.
        //    var user = await _uow.UserRepository.GetByEmail(email) ?? throw new Exception("Invalid email.");

        //    // Generates a new GUID as a reset token.
        //    var token = Guid.NewGuid().ToString();
        //    // Stores the token on the user entity.
        //    user.ResetPasswordToken = token;
        //    // Sets the token expiration time (1 hour from now).
        //    user.ResetPasswordExpires = DateTime.UtcNow.Add(_tokenExpiration);

        //    // Updates the user entity.
        //    _uow.UserRepository.Update(user);
        //    // Saves the changes.
        //    await _uow.CompleteAsync();

        //    // Appends the token to the reset link.
        //    resetLink = $"{resetLink}?token={token}";
        //    // Sends the password reset email.
        //    await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);

        //    // Returns true indicating success.
        //    return true;
        //}

        ///// <summary>
        ///// Logs out a user by ID.
        ///// </summary>
        ///// <param name="id">The user ID.</param>
        ///// <returns>A task representing the asynchronous operation, with the User object.</returns>
        //public UserGetModel Logout(int id)
        //{
        //    // Retrieves the user entity by ID.
        //    var User = _uow.UserRepository.GetById(id);
        //    var users = _mapper.Map<UserGetModel>(User);
        //    // Returns the user object (logout logic may be handled elsewhere).
        //    return users;
        //}

        ///// <summary>
        ///// Resets a user's password using a token.
        ///// </summary>
        ///// <param name="token">The reset token.</param>
        ///// <param name="newPassword">The new password.</param>
        ///// <returns>A task representing the asynchronous operation, with a boolean indicating success.</returns>
        //public async Task<bool> ResetPassword(string token, string newPassword)
        //{
        //    // Retrieves the user by the reset token.
        //    var user = await _uow.UserRepository.GetByResetToken(token);
        //    // Checks if token is invalid or expired.
        //    if (user == null || user.ResetPasswordExpires < DateTime.UtcNow)
        //    {
        //        // Throws exception for invalid token.
        //        throw new Exception("Invalid token");
        //    }

        //    // Creates a password hasher instance.
        //    PasswordHasher<User> _passwordHasher = new();
        //    // Hashes the new password.
        //    var hashedPassword = _passwordHasher.HashPassword(user, newPassword);

        //    // Updates the user's password.
        //    user.UserPassword = hashedPassword;
        //    // Clears the reset token.
        //    user.ResetPasswordToken = null;
        //    // Clears the token expiration.
        //    user.ResetPasswordExpires = null;

        //    // Updates the user entity.
        //    _uow.UserRepository.Update(user);
        //    // Saves the changes.
        //    await _uow.CompleteAsync();
        //    // Returns true indicating success.
        //    return true;
        //}

        /// <summary>
        /// Checks if a user has menu permissions based on group roles and screen IDs.
        /// </summary>
        /// <param name="groupRoles">The list of group role IDs.</param>
        /// <param name="menuUrl">The menu URL.</param>
        /// <param name="screenId">The list of screen IDs.</param>
        /// <returns>A task representing the asynchronous operation, with a boolean indicating permission.</returns>
        private async Task<bool> HasMenuPermissions(List<int> groupRoles, string menuUrl, List<int> screenId)
        {
            // Queries the Role repository for permissions.
            var menuPermissions = await _uow.RoleRepository.Query()
     // Filters for roles in the provided list.
     .Where(m => groupRoles.Contains(m.RoleId)
                 // Filters for roles associated with the provided screen IDs.
                 && screenId.Contains((int)m.ScreenId!)
                 // Filters for roles that have an action matching the menu URL (using SQL LIKE).
                 && EF.Functions.Like(m.RoleAction, "%" + menuUrl + "%"))
     .ToListAsync();

            // Returns true if any matching permissions were found.
            return menuPermissions.Count != 0;
        }

        /// <summary>
        /// Gets roles by a list of group models.
        /// </summary>
        /// <param name="groupIds">The list of group models.</param>
        /// <returns>A task representing the asynchronous operation, with a list of RoleModel.</returns>
        public async Task<List<RoleModel>> GetRolesByGroupIds(List<GroupModel> groupIds)
        {
            // Creates a HashSet to store unique role IDs.
            var roleIds = new HashSet<int>();

            // Fetch role IDs sequentially to avoid DbContext concurrency issue
            // Iterates through each group model.
            foreach (var groupId in groupIds)
            {
                // Gets the role IDs associated with the current group.
                var rolesForGroup = await _uow.GroupRoleRepository.GetGroupRoles(groupId.GroupId);
                // Iterates through each role ID for the group.
                foreach (var roleId in rolesForGroup)
                {
                    // Adds the role ID to the HashSet (ensures uniqueness).
                    roleIds.Add(roleId); // Ensures uniqueness
                }
            }

            // Ensure roleIds is not empty before querying roles
            // Checks if any role IDs were found.
            if (roleIds.Count == 0)
                // Returns an empty list if no roles.
                return [];

            // Fetch roles after roleIds list is fully populated
            // Queries the Role repository for the collected role IDs.
            var roles = await _uow.RoleRepository.Query()
                // Filters for roles in the collected ID set.
                .Where(role => roleIds.Contains(role.RoleId))
                // Projects the result into RoleModel objects.
                .Select(rp => new RoleModel
                {
                    // Maps role ID.
                    RoleId = rp.RoleId,
                    // Maps role action.
                    RoleAction = rp.RoleAction
                })
                .ToListAsync();

            // Returns the list of RoleModels.
            return roles;
        }

        /// <summary>
        /// Gets roles by group ID.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <returns>A task representing the asynchronous operation, with a list of RoleModel.</returns>
        public async Task<List<RoleModel>> GetRolesByGroupId(int groupId)
        {
            // Gets the role IDs associated with the specified group.
            var roleIds = await _uow.GroupRoleRepository.GetGroupRoles(groupId);
            // Queries the Role repository for those role IDs.
            var roles = await _uow.RoleRepository.Query()
                .Where(role => roleIds.Contains(role.RoleId))
                // Projects into RoleModel.
                .Select(rp => new RoleModel
                {
                    // Maps role ID.
                    RoleId = rp.RoleId,
                    // Maps role action.
                    RoleAction = rp.RoleAction
                })
                .ToListAsync();

            // Returns the list of roles.
            return roles;
        }
        public async Task<List<string>> GetUserPermissionsAsync(int userId)
        {
            var cacheKey = $"PERMISSIONS_USER_{userId}";

            if (_cache.TryGetValue(cacheKey, out List<string>? permissions))
                if (permissions != null)
                {
                    return permissions;
                }

            permissions = await (
           from ug in _uow.UserGroupRepository.Query()
           join gr in _uow.GroupRoleRepository.Query() on ug.GroupId equals gr.GroupId
           join r in _uow.RoleRepository.Query() on gr.RoleId equals r.RoleId
           where ug.UserId == userId && r.RoleAction != null
           select r.RoleAction).Distinct().ToListAsync();

            _cache.Set(cacheKey, permissions, TimeSpan.FromMinutes(30));
            return permissions;
        }
        public void RemoveUserPermissionsCache(int userId)
        {
            var cacheKey = $"PERMISSIONS_USER_{userId}";
            _cache.Remove(cacheKey);
        }
        /// <summary>
        /// Gets the user roles from a ClaimsPrincipal.
        /// </summary>
        /// <param name="user">The ClaimsPrincipal representing the user.</param>
        /// <returns>The user roles as a string.</returns>
        private static string? GetUserRoles(System.Security.Claims.ClaimsPrincipal user)
        {
            // Extracts claims of type Role from the user principal.
            return user.Claims
                // Filters for role claims.
                .Where(c => c.Type == ClaimTypes.Role)
                // Selects the value of the role claim.
                .Select(c => c.Value)
                // Returns the first role found (or null).
                .FirstOrDefault();
        }
    }
}
