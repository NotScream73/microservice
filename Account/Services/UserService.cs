using Account.Data;
using Account.Exceptions;
using Account.Models;
using Account.Services.DTO;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Account.Services
{
    public class UserService : IUserService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UserService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<int> CreateAsync(UserCreateDTO userDTO)
        {
            if (await _context.Users.AnyAsync(u => u.UserName == userDTO.UserName))
                throw new ApiException("Пользователь с таким именем уже существует.");

            var user = _mapper.Map<User>(userDTO);

            user.PasswordHash = UserPasswordService.ComputeHash(userDTO.Password);
            user.IsLogin = true;

            using var tr = await _context.Database.BeginTransactionAsync();

            await _context.Users.AddAsync(user);

            await _context.SaveChangesAsync();

            var role =
                await _context.Roles.Where(r => r.Name == "User")
                                    .Select(r => new
                                    {
                                        r.Id
                                    })
                                    .FirstOrDefaultAsync()
                                    ?? throw new ApiException("Роль \"Пользователь\" не существует.");

            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            };

            await _context.UserRoles.AddAsync(userRole);

            await _context.SaveChangesAsync();

            await tr.CommitAsync();

            return user.Id;
        }

        public async Task<int> AuthenticateAsync(string username, string password)
        {
            var passwordHash = UserPasswordService.ComputeHash(password);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == username && u.PasswordHash == passwordHash && !u.IsDeleted)
                ?? throw new ApiException("Неправильный логин или пароль.");

            user.IsLogin = true;

            await _context.SaveChangesAsync();

            return user.Id;
        }

        public async Task<User?> GetByIdAsync(int userId, bool? isDeleted, bool? isLogin)
        {
            var userQuery = _context.Users.Where(u => u.Id == userId);

            if (isDeleted.HasValue)
            {
                userQuery.Where(u => u.IsDeleted == isDeleted);
            }

            if (isLogin.HasValue)
            {
                userQuery.Where(u => u.IsLogin == isLogin);
            }

            return await userQuery.FirstOrDefaultAsync();
        }

        public async Task<AccountInformationListDTO> GetAllAsync(int from, int count)
        {
            var userQuery =
                (
                    from u in _context.Users
                    select new AccountInformationDTO
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        UserName = u.UserName,
                        IsDeleted = u.IsDeleted,
                        IsLogin = u.IsLogin,
                        Roles =
                            (
                                from ur in _context.UserRoles
                                join r in _context.Roles on ur.RoleId equals r.Id
                                where ur.UserId == u.Id
                                select r.Name
                            ).ToArray()
                    }
                );
            var totalCount = userQuery.Count();

            var userList = await userQuery.Skip(from).Take(count).ToListAsync();

            var result = new AccountInformationListDTO
            {
                Accounts = userList,
                TotalCount = totalCount
            };

            return result;
        }

        public async Task UpdateAsync(int userId, UserUpdateDTO userDTO)
        {
            var user = await GetByIdAsync(userId, false, null)
                ?? throw new NotFoundException($"Пользователь с ID {userId} не найден.");

            _mapper.Map(userDTO, user);

            user.PasswordHash = UserPasswordService.ComputeHash(userDTO.Password);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int userId)
        {
            var user = await GetByIdAsync(userId, false, null)
                ?? throw new NotFoundException($"Пользователь с ID {userId} не найден.");

            user.IsDeleted = true;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DoesUserExistAsync(int userId)
        {
            var user = await GetByIdAsync(userId, false, null);

            if (user == null)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> LogoutAsync(int userId)
        {
            var user = await GetByIdAsync(userId, false, null)
                ?? throw new NotFoundException($"Пользователь с ID {userId} не найден.");

            user.IsLogin = false;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<AccountMeInfoDTO> GetMeInfoByIdAsync(int userId)
        {
            var userInfo = await
                (
                    from u in _context.Users
                    where u.Id == userId && !u.IsDeleted && u.IsLogin
                    select new AccountMeInfoDTO
                    {
                        Id = u.Id,
                        LastName = u.LastName,
                        FirstName = u.FirstName,
                        UserName = u.UserName,
                        Roles =
                            (
                                from ur in _context.UserRoles
                                join r in _context.Roles on ur.RoleId equals r.Id
                                where ur.UserId == u.Id
                                select r.Name
                            ).ToArray()
                    }
                ).FirstOrDefaultAsync()
                ?? throw new NotFoundException($"Пользователь с ID {userId} не найден.");

            return userInfo;
        }

        public async Task<int> CreateByAdminAsync(UserCreateByAdminDTO userDTO)
        {
            if (await _context.Users.AnyAsync(u => u.UserName == userDTO.UserName))
                throw new ApiException("Пользователь с таким именем уже существует.");

            var user = _mapper.Map<User>(userDTO);

            user.PasswordHash = UserPasswordService.ComputeHash(userDTO.Password);
            user.IsLogin = true;

            using var tr = await _context.Database.BeginTransactionAsync();

            await _context.Users.AddAsync(user);

            await _context.SaveChangesAsync();

            var roles =
                await _context.Roles.Where(r => userDTO.Roles.Contains(r.Name))
                                    .Select(r => new
                                    {
                                        r.Id
                                    })
                                    .ToListAsync();

            if (!roles.Any())
            {
                throw new ApiException("Надо указать хотя бы одну сущетсвующую роль.");
            }

            var userRoles = roles.Select(r => new UserRole
            {
                RoleId = r.Id,
                UserId = user.Id
            });

            await _context.UserRoles.AddRangeAsync(userRoles);

            await _context.SaveChangesAsync();

            await tr.CommitAsync();

            return user.Id;
        }

        public async Task UpdateUserAsync(int userId, UserUpdateByAdminDTO userDTO)
        {
            var user = _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefault(u => u.Id == userId && !u.IsDeleted)
                ?? throw new NotFoundException($"Пользователь с ID {userId} не найден.");

            var existUserName = await _context.Users.AnyAsync(u => u.UserName == userDTO.Username && u.Id != userId);

            if (existUserName)
            {
                throw new ApiException("Имя пользователя уже занято.");
            }

            _mapper.Map(userDTO, user);

            user.PasswordHash = UserPasswordService.ComputeHash(userDTO.Password);

            var currentRoleIds = user.UserRoles.Select(ur => ur.Role.Id).ToList();

            var newRoleIds = _context.Roles.Where(r => userDTO.Roles.Contains(r.Name)).Select(i => i.Id).ToList();

            if (newRoleIds.Count != userDTO.Roles.Length)
            {
                throw new ApiException("Не удалось найти роль.");
            }

            var rolesToAdd = newRoleIds.Except(currentRoleIds).ToList();
            var rolesToRemove = currentRoleIds.Except(newRoleIds).ToList();

            foreach (var roleId in rolesToRemove)
            {
                var userRole = user.UserRoles.FirstOrDefault(ur => ur.Role.Id == roleId);
                if (userRole != null)
                {
                    _context.UserRoles.Remove(userRole);
                }
            }

            foreach (var roleId in rolesToAdd)
            {
                var newUserRole = new UserRole { UserId = userId, RoleId = roleId };
                await _context.UserRoles.AddAsync(newUserRole);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<string[]> GetUserRolesByIdAsync(int id)
        {
            var roles = await
                (
                    from u in _context.Users
                    join ur in _context.UserRoles on u.Id equals ur.UserId
                    join r in _context.Roles on ur.RoleId equals r.Id
                    where u.Id == id && !u.IsDeleted
                    select r.Name
                ).ToArrayAsync();

            if (roles.Length == 0)
            {
                throw new NotFoundException($"Пользователь с {id} не найден.");
            }

            return roles;
        }
    }
}