using MicroService.Data;
using MicroService.Models;
using MicroService.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MicroService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentsController(ApplicationDbContext context) => _context = context;

        // GET: api/Students
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
        {
            return await _context.Students.OrderByDescending(i => i.Id).ToListAsync();
        }

        // GET: api/Students/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            var student = await _context.Students.FirstOrDefaultAsync(i => i.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            return student;
        }

        // GET: api/Students/report
        [HttpGet("/report")]
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

        // PUT: api/Students/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudent(int id, Student student)
        {
            if (id != student.Id)
            {
                return BadRequest();
            }

            _context.Entry(student).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Students
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Student>> PostStudent(Student student)
        {
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStudent", new { id = student.Id }, student);
        }

        // DELETE: api/Students/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FirstOrDefaultAsync(i => i.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Students/5/expel
        [HttpPost("{id}/expel")]
        public async Task<IActionResult> ExpelStudent(int id)
        {
            var student = await _context.Students.FirstOrDefaultAsync(i => i.Id == id && !i.IsExpelled);

            if (student == null)
            {
                return BadRequest();
            }

            student.IsExpelled = true;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok();
        }

        // POST: api/Students/5/transfer
        [HttpPost("{id}/transfer")]
        public async Task<IActionResult> TransferStudent(int id, TransferStudentDTO transferStudentDTO)
        {
            var student = await _context.Students.FirstOrDefaultAsync(i => i.Id == id && !i.IsExpelled);

            if (student == null || string.IsNullOrEmpty(transferStudentDTO.Speciality))
            {
                return BadRequest();
            }

            student.Speciality = transferStudentDTO.Speciality;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok();
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}
