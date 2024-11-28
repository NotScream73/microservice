namespace Account.Services.DTO
{
    public class AccountInformationDTO
    {
        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string UserName { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsLogin { get; set; }
        public string[]? Roles { get; set; }
    }
}
