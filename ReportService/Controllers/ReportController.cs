using Domain.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportService.Models.DTO;

namespace ReportService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReportController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReportController(ApplicationDbContext context) => _context = context;

    // GET: api/Report
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReportDTO>>> GetReportStudents()
    {
        var report =
            await _context.Students
                .GroupBy(i => i.Speciality)
                .Select(i => new ReportDTO
                {
                    Speciality = i.Key,
                    Expelled = i.Count(i => i.IsExpelled),
                    Listed = i.Count(i => !i.IsExpelled)
                })
                .ToListAsync();

        return report;
    }
}
