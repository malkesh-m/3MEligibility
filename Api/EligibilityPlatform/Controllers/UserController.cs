using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EligibilityPlatform.Application.Attributes;
using EligibilityPlatform.Application.Constants;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace EligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing user operations including authentication and user management.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UserController"/> class.
    /// </remarks>
    /// <param name="userService">The user service.</param>
    /// <param name="configuration">The configuration instance.</param>
    [Route("api/user")]
    [ApiController]
    public class UserController(IUserService userService, IConfiguration configuration) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly IConfiguration _configuration = configuration;
        private readonly string _key = configuration["Jwt:Key"]!;
        private readonly string _issuer = configuration["Jwt:Issuer"]!;
        private readonly string _audience = configuration["Jwt:Audience"]!;
        private readonly double _expiryDuration = Convert.ToDouble(configuration["Jwt:ExpiresInMinutes"]);

        /// <summary>
        /// Retrieves all users for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the list of users.</returns>
        /// 
        //[RequirePermission("View Users Screen")]
        [Authorize(Policy = Permissions.User.View)]
        [HttpGet("getall")]
        public async Task<IActionResult> Get()
        {
            // Retrieves all users for the current entity
            ApiResponse<List<UserGetModel>> result = await _userService.GetAll(User.GetTenantId());
            // Returns success response with the list of users
            return Ok( result);
        }

    //    /// <summary>
    //    /// Retrieves a user by its unique identifier for the current entity.
    //    /// </summary>
    //    /// <param name="id">The unique identifier of the user.</param>
    //    /// <returns>An <see cref="IActionResult"/> containing the user if found; otherwise, not found.</returns>
    //    /// 
    //    [RequirePermission("View Users Screen")]

    //    [HttpGet("{id}")]
    //    public IActionResult Get(int id)
    //    {
    //        // Retrieves a specific user by ID for the current entity
    //        var result = _userService.GetById(User.GetTenantId(), id);
    //        // Checks if the user was found
    //        if (result != null)
    //        {
    //            // Returns success response with the user data
    //            return Ok(new ResponseModel { IsSuccess = false, Message = GlobalcConstants.Success, Data = result });
    //        }
    //        else
    //        {
    //            // Returns not found response when user doesn't exist
    //            return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
    //        }
    //    }

    //    /// <summary>
    //    /// Adds a new user for the current entity.
    //    /// </summary>
    //    /// <param name="userModel">The user model to add.</param>
    //    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    //    /// 
    //    [RequirePermission("add users")]

    //    [HttpPost]
    //    public async Task<IActionResult> Add(UserAddModel userModel)
    //    {
    //        // Sets the entity ID from the current user context
    //        userModel.EntityId = User.GetTenantId();
    //        var userName = User.GetUserName();
    //        // Validates the model state
    //        if (!ModelState.IsValid)
    //        {
    //            // Returns bad request if model validation fails
    //            return BadRequest(ModelState);
    //        }
    //        // Adds the new user with profile file
    //        await _userService.Add(userModel.UserProfileFile, userModel, userName!);
    //        // Returns success response after creation
    //        return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
    //    }

    //    /// <summary>
    //    /// Updates an existing user for the current entity.
    //    /// </summary>
    //    /// <param name="userModel">The user model to update.</param>
    //    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    //    /// 
    //    [RequirePermission("edit users")]

    //    [HttpPut]
    //    public async Task<IActionResult> Edit(UserEditModel userModel)
    //    {
    //        // Sets the entity ID from the current user context
    //        userModel.EntityId = userModel.EntityId;
    //        var userName = User.GetUserName();

    //        // Validates the model state
    //        if (!ModelState.IsValid)
    //        {
    //            // Returns bad request if model validation fails
    //            return BadRequest(ModelState);
    //        }
    //        // Updates the user with profile file
    //        await _userService.Update(userModel.UserProfileFile, userModel, userName!);
    //        // Returns success response after update
    //        return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
    //    }

    //    /// <summary>
    //    /// Updates the user's profile picture for the current entity.
    //    /// </summary>
    //    /// <param name="model">The user picture model.</param>
    //    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    //    [HttpPut("updatepicture")]
    //    public async Task<IActionResult> UpdatePicture(UserPictureModel model)
    //    {
    //        // Validates the model state
    //        if (!ModelState.IsValid)
    //        {
    //            // Returns bad request if model validation fails
    //            return BadRequest(ModelState);
    //        }
    //        // Updates the user's profile picture
    //        await _userService.UpdatePic(User.GetTenantId(), model.UserProfileFile, model);
    //        // Returns success response after update
    //        return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
    //    }

    //    /// <summary>
    //    /// Deactivates a user by its unique identifier for the current entity.
    //    /// </summary>
    //    /// <param name="id">The unique identifier of the user to deactivate.</param>
    //    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    //    /// 
    //    [RequirePermission("deactivate user")]

    //    [HttpDelete]
    //    public async Task<IActionResult> Deactivate(int id)
    //    {
    //        // Deactivates the user
    //        await _userService.Remove(User.GetTenantId(), id);
    //        // Returns success response after deactivation
    //        return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
    //    }

    //    /// <summary>
    //    /// Deactivates a user by its unique identifier for the current entity (alternative endpoint).
    //    /// </summary>
    //    /// <param name="id">The unique identifier of the user to deactivate.</param>
    //    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    //    /// 
    //    [RequirePermission("deactivate user")]

    //    [HttpDelete("deactivateuser")]
    //    public async Task<IActionResult> DeactivateUser([FromBody] List<int> ids)
    //    {
    //        if (ids == null || ids.Count == 0)
    //        {
    //            // Returns bad request if no IDs are provided
    //            return BadRequest(new ResponseModel { IsSuccess = false, Message = "No IDs provided." });
    //        }
    //        // Deactivates the user using alternative endpoint
    //        await _userService.RemoveUsers(User.GetTenantId(), ids);
    //        // Returns success response after deactivation
    //        return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
    //    }

    //    /*
    //    /// <summary>
    //    /// Suspends a user by its unique identifier.
    //    /// </summary>
    //    /// <param name="id">The unique identifier of the user to suspend.</param>
    //    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    //    [HttpDelete("SuspendUser")]
    //    public async Task<IActionResult> SuspendUser(int id)
    //    {
    //        await _userService.SuspendUser(id);
    //        return Ok(new ResponseModel { IsSuccess = true, Message = "Suspended user successfully" });
    //    }
    //    */

    //    /// <summary>
    //    /// Authenticates a user and returns a JWT token.
    //    /// </summary>
    //    /// <param name="loginModel">The login model containing credentials.</param>
    //    /// <returns>An <see cref="IActionResult"/> containing the authentication result and token.</returns>
    //    [HttpPost("login")]
    //    [AllowAnonymous]
    //    public async Task<IActionResult> Login(LoginModel loginModel)
    //    {
    //        // Validates the model state
    //        if (!ModelState.IsValid)
    //        {
    //            return BadRequest(ModelState);
    //        }

    //        // Checks if login credentials are provided
    //        if (loginModel == null || string.IsNullOrEmpty(loginModel.LoginId) || string.IsNullOrEmpty(loginModel.Password))
    //        {
    //            return Ok(new ResponseModel { IsSuccess = false, Message = "Email and password are required." });
    //        }

    //        // Authenticates the user
    //        var user = await _userService.AuthenticateUser(loginModel.LoginId, loginModel.Password);
    //        if (user == null)
    //        {
    //            return Unauthorized(new ResponseModel { IsSuccess = false, Message = "Invalid name or password." });
    //        }

    //        if (user.ForcePasswordChange)
    //        {
    //            return Ok(new
    //            {
    //                StatusCode = 200,
    //                ForcePasswordChange = true,
    //                Message = "Password expired. Please reset your password.",
    //                User = new
    //                {
    //                    user.UserId,
    //                    user.LoginId
    //                }
    //            });
    //        }

    //        // Get user groups
    //        var grouplist = _userService.GetGroupList(user.EntityId, user.UserId);

    //        // Check if user has groups
    //        if (grouplist == null || grouplist.Count == 0)
    //        {
    //            return StatusCode(500, new ResponseModel
    //            {
    //                IsSuccess = false,
    //                Message = "User has no assigned groups."
    //            });
    //        }

    //        // Get roles for groups
    //        var groupRole = await _userService.GetRolesByGroupIds(grouplist);

    //        if (user.SecurityGroup == null)
    //        {
    //            return StatusCode(500, new ResponseModel
    //            {
    //                IsSuccess = false,
    //                Message = "Security group not found."
    //            });
    //        }

    //        // Generate token
    //        string token;
    //        try
    //        {
    //            token = GenerateToken(user);
    //        }
    //        catch (Exception ex)
    //        {
    //            return StatusCode(500, new ResponseModel
    //            {
    //                IsSuccess = false,
    //                Message = $"Token generation failed: {ex.Message}"
    //            });
    //        }

    //        return Ok(new
    //        {
    //            StatusCode = 200,
    //            Token = token,
    //            User = new
    //            {
    //                user.UserId,
    //                user.UserName,
    //                user.LoginId,
    //                user.Email,
    //                user.Phone,
    //                user.Issuspended,
    //                GroupName = user.SecurityGroup.GroupName ?? ""
    //            },
    //            GroupRoles = groupRole ?? []
    //        });
    //    }
    //    /// <summary>
    //    /// Changes the password for a user.
    //    /// </summary>
    //    /// <param name="model">The change password model.</param>
    //    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    //    [HttpPost("changepassword"), AllowAnonymous]
    //    public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
    //    {
    //        // Validates the model state
    //        if (!ModelState.IsValid)
    //        {
    //            // Returns bad request if model validation fails
    //            return BadRequest(ModelState);
    //        }

    //        // Validates user ID
    //        if (model.UserId <= 0)
    //        {
    //            // Returns error response if user ID is invalid
    //            return Ok(new ResponseModel { IsSuccess = false, Message = "User not found." });
    //        }

    //        var currentUserId = model.UserId;

    //        // Verifies the current password
    //        var passwordVerifier = await _userService.VerifyPassword(currentUserId, model.CurrentPassword);
    //        if (!passwordVerifier)
    //        {
    //            // Returns error response if current password is incorrect
    //            return Ok(new ResponseModel { IsSuccess = false, Message = "Current password is incorrect." });
    //        }
    //        var isSameAsCurrent = await _userService.VerifyPassword(currentUserId, model.NewPassword);

    //        if (isSameAsCurrent)
    //        {
    //            return Ok(new ResponseModel
    //            {
    //                IsSuccess = false,
    //                Message = "New password cannot be the same as current password."
    //            });
    //        }

    //        // Changes the password
    //        var result = await _userService.ChangePassword(currentUserId, model.CurrentPassword, model.NewPassword);

    //        if (result)
    //        {
    //            // Returns success response after password change
    //            return Ok(new ResponseModel { IsSuccess = true, Message = "Password changed successfully." });
    //        }
    //        else
    //        {
    //            // Returns error response if password change fails
    //            return Ok(new ResponseModel { IsSuccess = false, Message = "An error occurred while changing the password." });
    //        }
    //    }

    //    /// <summary>
    //    /// Sends a password reset email to the user.
    //    /// </summary>
    //    /// <param name="model">The forgot password model.</param>
    //    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    //    [AllowAnonymous]
    //    [HttpPost("forgotpassword")]
    //    public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
    //    {
    //        // Validates the model state
    //        if (!ModelState.IsValid)
    //        {
    //            // Returns bad request if model validation fails
    //            return BadRequest(ModelState);
    //        }

    //        // Requests password reset
    //        var result = await _userService.RequestPasswordReset(model.Email, model.ResetLink);

    //        if (result)
    //        {
    //            // Returns success response if email sent successfully
    //            return Ok(new ResponseModel { IsSuccess = true, Message = "Password reset email sent." });
    //        }
    //        else
    //        {
    //            // Returns error response if email sending fails
    //            return Ok(new ResponseModel { IsSuccess = false, Message = "An error occurred while processing your request." });
    //        }
    //    }

    //    /// <summary>
    //    /// Requests a password reset for the specified email.
    //    /// </summary>
    //    /// <param name="email">The user's email address.</param>
    //    /// <param name="resetLink">The reset link to send.</param>
    //    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    //    [AllowAnonymous]
    //    [HttpPost("requestresetpassword")]
    //    public async Task<IActionResult> RequestResetPassword(string email, string resetLink)
    //    {
    //        // Requests password reset with provided email and reset link
    //        var result = await _userService.RequestPasswordReset(email, resetLink);

    //        if (result)
    //        {
    //            // Returns success response if email sent successfully
    //            return Ok(new ResponseModel { IsSuccess = true, Message = "Password reset email sent." });
    //        }
    //        else
    //        {
    //            // Returns error response if email sending fails
    //            return Ok(new ResponseModel { IsSuccess = false, Message = "An error occurred while processing your request." });
    //        }
    //    }

    //    /// <summary>
    //    /// Logs out the user with the specified ID.
    //    /// </summary>
    //    /// <param name="id">The user ID to log out.</param>
    //    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    //    [HttpPost("Logout/{id}")]
    //    public IActionResult Logout(int id)
    //    {
    //        // Logs out the user
    //        var result = _userService.Logout(id);
    //        if (result != null)
    //        {
    //            // Returns success response after logout
    //            return Ok(new ResponseModel { IsSuccess = true, Message = "User Logout Successfully", Data = result });
    //        }
    //        else
    //        {
    //            // Returns error response if user not found
    //            return Ok(new ResponseModel { IsSuccess = false, Message = "No User Found" });
    //        }
    //    }

    //    /// <summary>
    //    /// Resets the password for a user using a token.
    //    /// </summary>
    //    /// <param name="model">The reset password model.</param>
    //    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    //    [AllowAnonymous]
    //    [HttpPost("resetpassword")]
    //    public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
    //    {
    //        // Validates the model state
    //        if (!ModelState.IsValid)
    //        {
    //            // Returns bad request if model validation fails
    //            return BadRequest(ModelState);
    //        }

    //        // Resets the password using the provided token
    //        var result = await _userService.ResetPassword(model.Token, model.NewPassword);

    //        if (result)
    //        {
    //            // Returns success response after password reset
    //            return Ok(new ResponseModel { IsSuccess = true, Message = "Password has been reset successfully." });
    //        }
    //        else
    //        {
    //            // Returns error response if password reset fails
    //            return Ok(new ResponseModel { IsSuccess = false, Message = "This email does not exist." });
    //        }
    //    }

    //    [RequirePermission("Reactivate User")]

    //    [HttpPost("reactivateuser")]
    //    public async Task<IActionResult> ReActivateUser(ReActivationModel model)
    //    {
    //        // Validates the model state
    //        if (!ModelState.IsValid)
    //        {
    //            // Returns bad request if model validation fails
    //            return BadRequest(ModelState);
    //        }

    //        // Resets the password using the provided token
    //        var result = await _userService.ReActivateUser(model);

    //        if (result != null)
    //        {
    //            // Returns success response after password reset
    //            return Ok(new ResponseModel { IsSuccess = true, Message = result });
    //        }
    //        else
    //        {
    //            // Returns error response if password reset fails
    //            return Ok(new ResponseModel { IsSuccess = false });
    //        }
    //    }


    //    /// <summary>
    //    /// Generates a JWT token for the specified user.
    //    /// </summary>
    //    /// <param name="user">The user entity.</param>
    //    /// <returns>The generated JWT token as a string.</returns>
    //    string GenerateToken(User user)
    //    {
    //        ArgumentNullException.ThrowIfNull(user);

    //        if (string.IsNullOrWhiteSpace(_key))
    //            throw new Exception("JWT Key is missing");

    //        if (string.IsNullOrWhiteSpace(_issuer))
    //            throw new Exception("JWT Issuer is missing");

    //        if (string.IsNullOrWhiteSpace(_audience))
    //            throw new Exception("JWT Audience is missing");

    //        var claims = new List<Claim>
    //{
    //    new("UserId", user.UserId.ToString()),
    //    new(ClaimTypes.Name, user.UserName ?? string.Empty),
    //    new(ClaimTypes.Email, user.Email ?? string.Empty),
    //    new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
    //    new("EntityId", user.EntityId.ToString())
    //};

    //        if (user.SecurityGroup != null)
    //            claims.Add(new Claim(ClaimTypes.Role, user.SecurityGroup.GroupId.ToString()));

    //        var securityKey = new SymmetricSecurityKey(
    //            Encoding.UTF8.GetBytes(_key)
    //        );

    //        var token = new JwtSecurityToken(
    //            issuer: _issuer,
    //            audience: _audience,
    //            claims: claims,
    //            expires: DateTime.UtcNow.AddMinutes(_expiryDuration),
    //            signingCredentials: new SigningCredentials(
    //                securityKey, SecurityAlgorithms.HmacSha256)
    //        );

    //        return new JwtSecurityTokenHandler().WriteToken(token);
    //    }
    }
}
