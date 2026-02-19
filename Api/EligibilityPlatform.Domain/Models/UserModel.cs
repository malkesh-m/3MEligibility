using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;

namespace MEligibilityPlatform.Domain.Models
{
    public class UserModel
    {
        public int UserId { get; set; }
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$",
            ErrorMessage = "User Name can contain Arabic letters, English letters, numbers, spaces, hyphens, and underscores only.")]
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
        public required string UserName { get; set; }

        [Required(ErrorMessage = "Login ID is required.")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Login ID must be between 5 and 50 characters.")]
        public required string LoginId { get; set; }
        [JsonIgnore]
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public required string UserPassword { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public required string Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string? Phone { get; set; }

        //[Range(1, int.MaxValue, ErrorMessage = "Entity ID must be a positive integer.")]
        public int EntityId { get; set; }

        [SwaggerIgnore]
        public byte[]? UserPicture { get; set; }
        public int TenantId { get; set; }

        [NotMapped]
        public IFormFile? UserProfileFile { get; set; }

        [SwaggerIgnore]
        public int? StatusId { get; set; }
        public bool? Issuspended { get; set; }

    }
    public class ApiResponse<T>
    {
        public T Data { get; set; } = default!;
        public List<string> Messages { get; set; } = [];
        public bool Succeeded { get; set; }
    }
    public class UserGetModel
    {
        public int Id { get; set; }
        public int TenantId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public string PhotoFilePath { get; set; } = string.Empty;
        public string PhotoFileName { get; set; } = string.Empty;

        public string MobileNo { get; set; } = string.Empty;

        public int CountryId { get; set; }
        [JsonPropertyName("stateId")]
        public int? StateId { get; set; }

        public int? CityId { get; set; }

        public int LanguageId { get; set; }

        public string? TimeZone { get; set; }

        public int GenderId { get; set; }

        public string Culture { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        // Roles from local database (enriched)
        public List<RoleModel> Roles { get; set; } = [];
    }

    public class UserAddModel : UserModel
    {


    }

    public class UserEditModel
    {

        public int UserId { get; set; }

        [RegularExpression(
          @"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$",
            ErrorMessage = "User Name can contain Arabic letters, English letters, numbers, spaces, hyphens, and underscores only."
            )]
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
        public required string UserName { get; set; }

        [Required(ErrorMessage = "Login ID is required.")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Login ID must be between 5 and 50 characters.")]
        public required string LoginId { get; set; }


        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public required string Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string? Phone { get; set; }

        //[Range(1, int.MaxValue, ErrorMessage = "Entity ID must be a positive integer.")]
        public int EntityId { get; set; }
        public int TenantId { get; set; }

        [SwaggerIgnore]
        public byte[]? UserPicture { get; set; }

        [NotMapped]
        public IFormFile? UserProfileFile { get; set; }

        [SwaggerIgnore]
        public int? StatusId { get; set; }
        public bool? Issuspended { get; set; }

    }
    public class ReActivationModel
    {
        public int UserId { get; set; }
        public bool IsActivation { get; set; }
    }
    public class UserPictureModel
    {
        public int UserId { get; set; }

        [SwaggerIgnore]
        public byte[]? UserPicture { get; set; }

        [NotMapped]
        public IFormFile? UserProfileFile { get; set; }
    }
    public class RoleModel
    {
        public int RoleId { get; set; }
        public required string RoleName { get; set; }
    }
}
