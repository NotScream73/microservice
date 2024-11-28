namespace Account.Services.DTO;

public class UserUpdateDTO
{
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string Password { get; set; }
}

public class UserUpdateByAdminDTO : UserUpdateDTO
{
    public string Username { get; set; }
    public string[] Roles { get; set; }
}
