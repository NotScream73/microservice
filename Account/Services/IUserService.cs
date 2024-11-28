using Account.Models;
using Account.Services.DTO;

namespace Account.Services
{
    public interface IUserService
    {
        Task<int> CreateAsync(UserCreateDTO userDTO);
        Task<int> CreateByAdminAsync(UserCreateByAdminDTO userDTO);
        Task<int> AuthenticateAsync(string username, string password);
        Task<bool> LogoutAsync(int userId);
        Task<User> GetByIdAsync(int userId, bool? isDeleted, bool? isLogin);
        Task<AccountMeInfoDTO> GetMeInfoByIdAsync(int userId);
        Task<AccountInformationListDTO> GetAllAsync(int from, int count);
        Task UpdateAsync(int userId, UserUpdateDTO user);
        Task DeleteAsync(int userId);
        Task<bool> DoesUserExistAsync(int userId);
        Task UpdateUserAsync(int userId, UserUpdateByAdminDTO userDTO);
        Task<string[]> GetUserRolesByIdAsync(int id);
    }
}