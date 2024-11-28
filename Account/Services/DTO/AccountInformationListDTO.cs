namespace Account.Services.DTO
{
    public class AccountInformationListDTO
    {
        public List<AccountInformationDTO> Accounts { get; set; }
        public int TotalCount { get; set; }
    }
}
