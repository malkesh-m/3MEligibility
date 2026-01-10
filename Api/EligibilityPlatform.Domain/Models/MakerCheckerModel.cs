using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using EligibilityPlatform.Domain.Enums;

namespace EligibilityPlatform.Domain.Models
{
    public class MakerCheckerModel
    {
        public int MakerCheckerId { get; set; }

        public int MakerId { get; set; }

        //public string MakerName { get; set; }

        public int? CheckerId { get; set; }

        public DateTime MakerDate { get; set; }

        public DateTime? CheckerDate { get; set; }

        public required string TableName { get; set; }

        public required string ActionName { get; set; }

        public required string OldValueJson { get; set; }

        public required string NewValueJson { get; set; }

        public int RecordId { get; set; }

        public required MakerCheckerStatusEnum Status { get; set; }

        public required string StatusName { get; set; }

        public DateTime UpdatedByDateTime { get; set; }

        public string? Comment { get; set; }

        public string? CheckerName { get; set; }
        public string? MakerName { get; set; }
    }

    //public class MakerCheckerGetModel : MakerCheckerModel
    //{
    //    public string CheckerName { get; set; }
    //    public string MakerName { get; set; }   

    //}


    public class MakerCheckerAddUpdateModel
    {
        public int MakerCheckerId { get; set; }

        [JsonIgnore]
        public int MakerId { get; set; }

        //public string MakerName { get; set; }
        [JsonIgnore]

        public int? CheckerId { get; set; }
        [JsonIgnore]

        public DateTime MakerDate { get; set; }
        [JsonIgnore]

        public DateTime? CheckerDate { get; set; }

        [Required(ErrorMessage = "TableName is required.")]
        [StringLength(500, ErrorMessage = "TableName cannot exceed 500 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s_\-]+$", ErrorMessage = "TableName contains invalid characters.")]
        public required string TableName { get; set; }


        [Required(ErrorMessage = "ActionName is required.")]
        [StringLength(500, ErrorMessage = "ActionName cannot exceed 500 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s_\-]+$", ErrorMessage = "ActionName contains invalid characters.")]
        public required string ActionName { get; set; }


        [Required(ErrorMessage = "OldValueJson is required.")]
        public required string OldValueJson { get; set; }

        [Required(ErrorMessage = "NewValueJson is required.")]
        public required string NewValueJson { get; set; }

        public int RecordId { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [EnumDataType(typeof(MakerCheckerStatusEnum), ErrorMessage = "Invalid Status.")]
        public required MakerCheckerStatusEnum Status { get; set; }

        [Required(ErrorMessage = "StatusName is required.")]
        [StringLength(50, ErrorMessage = "StatusName cannot exceed 50 characters.")]
        public required string StatusName { get; set; }
        [JsonIgnore]

        public DateTime UpdatedByDateTime { get; set; }
        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters.")]
        public string? Comment { get; set; }
        [JsonIgnore]

        public string? CheckerName { get; set; }
        [JsonIgnore]

        public string? MakerName { get; set; }
    }
    public class MakerCheckerModelCopy
    {
        public int MakerCheckerId { get; set; }

        public int MakerId { get; set; }

        public int? CheckerId { get; set; }

        public DateTime MakerDate { get; set; }

        public DateTime? CheckerDate { get; set; }

        public required string TableName { get; set; }

        public required string ActionName { get; set; }

        public required string OldValueJson { get; set; }

        public required string NewValueJson { get; set; }

        public int RecordId { get; set; }

        public MakerCheckerStatusEnum Status { get; set; }
        public DateTime UpdatedByDateTime { get; set; }

        public string? Comment { get; set; }
    }
}
