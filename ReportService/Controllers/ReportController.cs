using Domain.Data;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ReportService.Models.DTO;

namespace ReportService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReportController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private static HttpClient httpClient = new HttpClient();

    public ReportController(ApplicationDbContext context) => _context = context;

    // GET: api/Report
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReportDTO>>> GetReportStudents()
    {
        using HttpResponseMessage response = await httpClient.GetAsync("http://app:8080/api/students/");

        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();

        var students = JsonConvert.DeserializeObject<List<Student>>(jsonString);

        if (students == null || students.Count == 0)
        {
            return Ok("Нет студентов");
        }

        var report =
            students
                .GroupBy(i => i.Speciality)
                .Select(i => new ReportDTO
                {
                    Speciality = i.Key,
                    Expelled = i.Count(i => i.IsExpelled),
                    Listed = i.Count(i => !i.IsExpelled)
                })
                .ToList();

        return report;
    }
}
