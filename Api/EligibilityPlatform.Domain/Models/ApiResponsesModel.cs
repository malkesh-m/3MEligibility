using System.ComponentModel.DataAnnotations;

namespace MEligibilityPlatform.Domain.Models
{
    public class ApiResponsesModel
    {
        public int ResponceId { get; set; }
        [Required]
        public int ApiId { get; set; } // Must be >0

        [Required]
        [StringLength(5000, ErrorMessage = "ResponceCode cannot exceed 5000 characters.")]
        public string ResponceCode { get; set; } = "";

        [StringLength(5000, ErrorMessage = "ResponceDescription cannot exceed 5000 characters.")]
        public string ResponceDescription { get; set; } = "";

        [Required]
        [StringLength(10000, ErrorMessage = "ResponceSchema cannot exceed 10000 characters.")]
        public string ResponceSchema { get; set; } = "";



    }

    public class ApiResponsesListModel : ApiResponsesModel
    {
        public DateTime CreatedByDateTime { get; set; }
        public DateTime UpdatedByDateTime { get; set; }
    }

    public class ApiResponsesCreateUpdateModel : ApiResponsesModel
    {

    }
}
