using System.ComponentModel.DataAnnotations;

namespace MEligibilityPlatform.Domain.Models
{
    public class AppSettingModel
    {
        /// <summary>
        /// Alwasy pass 1 for AppSettingId
        /// </summary>
        public int AppSettingId { get; set; }
        [Range(1, 10000, ErrorMessage = "MaximumEntities must be between 1 and 10000")]
        public int MaximumEntities { get; set; }
    }
}
