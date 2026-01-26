namespace MEligibilityPlatform.Domain.Models
{
    public class UserInfo
    {
        public int UserId { get; set; }
        public int GroupId { get; set; }
        public required string UserName { get; set; }
        public required string MobileNo { get; set; }

        //public required string LoginId { get; set; }
        public required string Email { get; set; }
        //public required string EntityName { get; set; }
    }
}
