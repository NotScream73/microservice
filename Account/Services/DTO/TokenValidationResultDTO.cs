namespace Account.Services.DTO;

public class TokenValidationResultDTO
{
    public bool IsValid { get; set; }
    public int? UserId { get; set; }
    public string[]? Roles { get; set; }
}
