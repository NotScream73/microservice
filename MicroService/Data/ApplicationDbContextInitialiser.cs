using MicroService.Models;
using Microsoft.EntityFrameworkCore;

namespace MicroService.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();

        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ApplicationDbContext _context;

    public ApplicationDbContextInitialiser(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task InitialiseAsync()
    {
        await _context.Database.MigrateAsync();
    }

    public async Task SeedAsync()
    {
        await TrySeedAsync();
    }

    public async Task TrySeedAsync()
    {
        if (!_context.Students.Any())
        {
            _context.Students.Add(new Student
            {
                FirstName = "Никита",
                LastName = "Сергеев",
                MiddleName = "Игоревич",
                Speciality = "Программный инженер",
                IsExpelled = false
            });

            await _context.SaveChangesAsync();
        }
    }
}