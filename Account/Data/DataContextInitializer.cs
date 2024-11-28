using Microsoft.EntityFrameworkCore;
using Account.Models;
using Account.Services;

namespace Account.Data;

public static class InitializerExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initializer = scope.ServiceProvider.GetRequiredService<DataContextInitializer>();

        await initializer.InitializeAsync();

        await initializer.SeedAsync();
    }
}

public class DataContextInitializer
{
    private readonly DataContext _context;

    public DataContextInitializer(DataContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync()
    {
        await _context.Database.MigrateAsync();
    }

    public async Task SeedAsync()
    {
        await TrySeedAsync();
    }

    public async Task TrySeedAsync()
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await TrySeedRolesAsync();

            await TrySeedUsersAsync();

            await TrySeedUserRolesAsync();

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
        }
    }

    private async Task TrySeedRolesAsync()
    {
        if (!await _context.Roles.AnyAsync())
        {
            await _context.Roles.AddRangeAsync(
                new Role { Name = "Admin" },
                new Role { Name = "Manager" },
                new Role { Name = "Doctor" },
                new Role { Name = "User" }
            );

            await _context.SaveChangesAsync();
        }
    }

    private async Task TrySeedUsersAsync()
    {
        if (!await _context.Users.AnyAsync())
        {
            var users = new List<User>
            {
                new()
                {
                    LastName = "Админов",
                    FirstName = "Админ",
                    UserName = "admin",
                    IsDeleted = false,
                },
                new() {
                    LastName = "Менджеров",
                    FirstName = "Менеджер",
                    UserName = "manager",
                    IsDeleted = false,
                },
                new()
                {
                    LastName = "Докторов",
                    FirstName = "Доктор",
                    UserName = "doctor",
                    IsDeleted = false,
                },
                new()
                {
                    LastName = "Пользователев",
                    FirstName = "Пользователь",
                    UserName = "user",
                    IsDeleted = false,
                }
            };

            foreach (var user in users)
            {
                user.PasswordHash = UserPasswordService.ComputeHash(user.UserName);
            }

            await _context.Users.AddRangeAsync(users);

            await _context.SaveChangesAsync();
        }
    }

    private async Task TrySeedUserRolesAsync()
    {
        if (!await _context.UserRoles.AnyAsync())
        {
            var userRoles = new List<UserRole>();

            var users = await _context.Users.ToListAsync();
            var roles = await _context.Roles.ToListAsync();

            var rolesInLower = roles.Select(x => x.Name.ToLower());
            foreach (var user in users)
            {
                if (rolesInLower.Contains(user.UserName))
                {
                    userRoles.Add(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = roles.First(i => i.Name.ToLower() == user.UserName).Id
                    });
                }
            }

            await _context.UserRoles.AddRangeAsync(userRoles);

            await _context.SaveChangesAsync();
        }
    }
}
