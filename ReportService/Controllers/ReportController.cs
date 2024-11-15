using Domain.Data;
using Domain.Models;
using MassTransit;
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
    private readonly ExternalClientService _externalClientService;
    private readonly IBus _bus;

    public ReportController(ApplicationDbContext context, ILogger<ReportController> logger, ExternalClientService externalClientService, IBus bus)
    {
        _context = context;
        _logger = logger;
        _externalClientService = externalClientService;
        _bus = bus;
    }

    // GET: api/Report
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReportDTO>>> GetReportStudents()
    {
        var response = await _externalClientService.SendRequestAsync(Request, "http://app:8080/api/students/", HttpMethod.Get, null, null);

        var students = JsonConvert.DeserializeObject<List<Student>>(response);

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

        await _bus.Publish(new MessageDTO
        {
            File = csvContent,
            FileName = HttpContext.Request.Headers["X-Trace-Id"].ToString()
        });

        //response = await _externalClientService.SendRequestAsync(Request, "http://storageservice:8080/api/storage/upload", HttpMethod.Post, null, content);

        return report;
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

public interface IOrderSubmitted
{
    string File { get; }
}
