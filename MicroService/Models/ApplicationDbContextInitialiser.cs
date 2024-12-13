using Domain.Data;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MicroService.Models;
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
                IsExpelled = false,
                Sort = 0
            });

            await _context.SaveChangesAsync();
        }

        _context.Students.AddRange(Enumerable.Range(1, 10000).Select(i => new Student
        {
            FirstName = "Имя",
            LastName = "Фамилия",
            MiddleName = "Отчество",
            Speciality = "Какая-то специальнсоть",
            IsExpelled = false,
            Sort = new Random().Next(1, 1000)
        }));

        await _context.SaveChangesAsync();
    }
}