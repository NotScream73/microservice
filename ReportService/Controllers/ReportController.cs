using Domain.Data;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ReportService.Models.DTO;
using System.Text;

namespace ReportService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReportController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private static HttpClient httpClient = new HttpClient();
    private readonly ILogger<ReportController> _logger;

    public ReportController(ApplicationDbContext context, ILogger<ReportController> logger)
    {
        _context = context;
        _logger = logger;
    }

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
            return NotFound("Нет студентов");
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

        var csvContent = GenerateCsv(report);

        var fileName = $"report_{DateTime.Now:yyyyMMddHHmmss}.csv";
        var filePath = Path.Combine(Path.GetTempPath(), fileName);
        await System.IO.File.WriteAllTextAsync(filePath, csvContent, Encoding.UTF8);

        var content = new MultipartFormDataContent
        {
            { new StreamContent(System.IO.File.OpenRead(filePath)), "file", fileName }
        };

        var csvResponse = await httpClient.PostAsync("http://storageservice:8080/api/storage/upload", content);

        if (csvResponse.IsSuccessStatusCode)
        {
            return report;
        }
        else
        {
            return BadRequest();
        }
    }
    private string GenerateCsv<T>(IEnumerable<T> data)
    {
        var csvBuilder = new StringBuilder();
        var properties = typeof(T).GetProperties();

        csvBuilder.AppendLine(string.Join(";", properties.Select(p => p.Name)));

        foreach (var item in data)
        {
            var values = properties.Select(p => p.GetValue(item, null)?.ToString() ?? string.Empty);
            csvBuilder.AppendLine(string.Join(";", values));
        }

        return csvBuilder.ToString();
    }

}
