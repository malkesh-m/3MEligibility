//using System.ComponentModel.DataAnnotations;
//using System.Text.Json.Serialization;

//namespace EligibilityPlatform.Domain.Models
//{
//    public class EntityModel
//    {
//        [Key]
//        public int EntityId { get; set; }
//        [Required]
//        public string? EntityName { get; set; }
//        [Required]
//        public int? CountryId { get; set; }
//        [Required]
//        public int? CityId { get; set; }
//        [Required]
//        public string? EntityAddress { get; set; }

//        public int? BaseCurrencyId { get; set; }

//        public string? Code { get; set; }

//        public string? Entitylocation { get; set; }

//        public bool Isparent { get; set; }
//        [Required]
//        public int? ParentEnitityId { get; set; }
//        public DateTime UpdatedByDateTime { get; set; }
//        public string? CreatedBy { get; set; }
//        public DateTime CreatedByDateTime { get; set; }
//        public string? UpdatedBy { get; set; }
//    }

//    public class CreateOrUpdateEntityModel
//    {
//        [Key]
//        public int EntityId { get; set; }
//        [Required(ErrorMessage = "Entity name is required.")]
//        [StringLength(50, ErrorMessage = "Entity name cannot exceed 50 characters.")]
//        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$", ErrorMessage = "Entity name contains invalid characters.")]
//        public required string EntityName { get; set; }
//        [Required(ErrorMessage = "CountryId is required.")]
//        [Range(1, int.MaxValue, ErrorMessage = "CountryId must be greater than 0.")]
//        public int? CountryId { get; set; }
//        [Required(ErrorMessage = "CityId is required.")]
//        [Range(1, int.MaxValue, ErrorMessage = "CityId must be greater than 0.")]
//        public int? CityId { get; set; }
//        [Required(ErrorMessage = "Entity address is required.")]
//        [StringLength(250, ErrorMessage = "Entity address cannot exceed 250 characters.")]
//        public string? EntityAddress { get; set; }

//        public int? BaseCurrencyId { get; set; }

//        [StringLength(50, ErrorMessage = "Entity location cannot exceed 50 characters.")]
//        public string? Entitylocation { get; set; }

//        public bool Isparent { get; set; }
//        [Required(ErrorMessage = "ParentEntityId is required.")]

//        public int? ParentEnitityId { get; set; }
//        [JsonIgnore]
//        public string? CreatedBy { get; set; }
//        [JsonIgnore]
//        public string? UpdatedBy { get; set; }
//        [StringLength(10, ErrorMessage = "Code cannot exceed 10 characters.")]
//        public string? Code { get; set; }

//    }

//    public class EntityModelDescription
//    {

//        public int EntityId { get; set; }

//        public string? EntityName { get; set; }

//        public int? CountryId { get; set; }
//        public string? CountryName { get; set; }

//        public int? CityId { get; set; }
//        public string? CityName { get; set; }

//        public string? EntityAddress { get; set; }

//        public string? Code { get; set; }

//        public bool Isparent { get; set; }

//        public int? ParentEnitityId { get; set; }
//    }

//}
