namespace Account.Controllers.DTO
{
    public class ValidationTokenResult
    {
        public bool IsValid { get; set; }
        public int? UserId { get; set; }
        public string[]? Roles { get; set; }
    }
}
