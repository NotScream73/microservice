namespace Account.Services.DTO;

public class UserCreateDTO
{
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}

public class UserCreateByAdminDTO : UserCreateDTO
{
    public string[] Roles { get; set; }
}