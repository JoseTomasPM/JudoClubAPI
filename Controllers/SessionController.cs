using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JudoClubAPI.Data;
using JudoClubAPI.DTOs;
using JudoClubAPI.Helpers;
using JudoClubAPI.Models;

namespace JudoClubAPI.Controllers;

[ApiController]
[Route("api/sessions")]
[Authorize]
public class SessionController : ControllerBase
{
    private readonly AppDbContext _db;

    public SessionController(AppDbContext db) => _db = db;

    // GET api/sessions
    // Todos pueden ver las sesiones
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var sessions = await _db.Sesions
            .Select(s => new SessionDto
            {
                Id = s.Id,
                Date = s.Date,
                Description = s.Description,
                Category = s.Category.ToString(),
                StudentCount = s.SesionStudents.Count
            })
            .ToListAsync();

        return Ok(sessions);
    }

    // GET api/sessions/{id}
    // Todos pueden ver el detalle de una sesión
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var session = await _db.Sesions
            .Where(s => s.Id == id)
            .Select(s => new SessionDto
            {
                Id = s.Id,
                Date = s.Date,
                Description = s.Description,
                Category = s.Category.ToString(),
                StudentCount = s.SesionStudents.Count
            })
            .FirstOrDefaultAsync();

        if (session == null)
            return NotFound($"No existe ninguna sesión con Id {id}.");

        return Ok(session);
    }

    // POST api/sessions — ahora asigna alumnos por categoría automáticamente
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateSessionDto dto)
    {
        var session = new Sesion
        {
            Date = dto.Date.ToUniversalTime(),
            Description = dto.Description,
            Category = dto.Category,
        };

        _db.Sesions.Add(session);
        await _db.SaveChangesAsync();

        // Auto-asignar alumnos de esa categoría
        var students = await _db.Students
            .Where(s => s.Category == dto.Category)
            .ToListAsync();

        foreach (var s in students)
        {
            _db.SesionStudents.Add(new SesionStudent
            {
                SesionId = session.Id,
                StudentId = s.Id,
                Attended = false
            });
        }

        await _db.SaveChangesAsync();

        return StatusCode(201, new SessionDto
        {
            Id = session.Id,
            Date = session.Date,
            Description = session.Description,
            Category = session.Category.ToString(),
            StudentCount = students.Count
        });
    }

    // PUT api/sessions 
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, CreateSessionDto dto)
    {
        var session = await _db.Sesions.FindAsync(id);
        if (session == null)
            return NotFound($"No existe ninguna sesión con Id {id}.");

        session.Date = dto.Date.ToUniversalTime();
        session.Description = dto.Description;
        session.Category = dto.Category;
        await _db.SaveChangesAsync();

        return Ok(new SessionDto
        {
            Id = session.Id,
            Date = session.Date,
            Description = session.Description,
            Category = session.Category.ToString(),
            StudentCount = await _db.SesionStudents.CountAsync(ss => ss.SesionId == id)
        });
    }

    // DELETE api/sessions/{id} — solo Admin
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var session = await _db.Sesions.FindAsync(id);
        if (session == null)
            return NotFound($"No existe ninguna sesión con Id {id}.");

        _db.Sesions.Remove(session);
        await _db.SaveChangesAsync();
        return NoContent();
    }


    // GET api/sessions/{id}/students
    // Admin ve todos / User solo si alguno de sus alumnos está en la sesión
    [HttpGet("{id}/students")]
    public async Task<IActionResult> GetStudents(int id)
    {
        var session = await _db.Sesions.FindAsync(id);
        if (session == null)
            return NotFound($"No existe ninguna sesión con Id {id}.");

        var myId = UserHelper.GetUserId(User);
        var isAdmin = UserHelper.IsAdmin(User);

        // Construimos la query base con filtro de acceso
        var query = _db.SesionStudents
            .Where(ss => ss.SesionId == id)
            .Where(ss => isAdmin || ss.Student.UserId == myId)
            .Include(ss => ss.Student);

        var students = await query
            .Select(ss => new SessionStudentDto
            {
                StudentId = ss.StudentId,
                Name = ss.Student.Name,
                Belt = ss.Student.Belt.ToString(),
                Attended = ss.Attended
            })
            .ToListAsync();

        return Ok(students);
    }

    // POST api/sessions/{id}/students/{studentId} — solo Admin
    [HttpPost("{id}/students/{studentId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddStudent(int id, int studentId)
    {
        var session = await _db.Sesions.FindAsync(id);
        if (session == null)
            return NotFound($"No existe ninguna sesión con Id {id}.");

        var student = await _db.Students.FindAsync(studentId);
        if (student == null)
            return NotFound($"No existe ningún alumno con Id {studentId}.");

        // Verificar que no está ya asignado
        var exists = await _db.SesionStudents
            .AnyAsync(ss => ss.SesionId == id && ss.StudentId == studentId);
        if (exists)
            return Conflict("Este alumno ya está asignado a esta sesión.");

        _db.SesionStudents.Add(new SesionStudent
        {
            SesionId = id,
            StudentId = studentId
        });
        await _db.SaveChangesAsync();

        return StatusCode(201, new { message = $"Alumno {student.Name} añadido a la sesión." });
    }

    // DELETE api/sessions/{id}/students/{studentId} — solo Admin
    [HttpDelete("{id}/students/{studentId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveStudent(int id, int studentId)
    {
        var link = await _db.SesionStudents
            .FirstOrDefaultAsync(ss => ss.SesionId == id && ss.StudentId == studentId);

        if (link == null)
            return NotFound("Este alumno no está asignado a esta sesión.");

        _db.SesionStudents.Remove(link);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // PUT api/sessions/{id}/students/{studentId}/attendance — NUEVO
    [HttpPut("{id}/students/{studentId}/attendance")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MarkAttendance(int id, int studentId, AttendanceDto dto)
    {
        var link = await _db.SesionStudents
            .FirstOrDefaultAsync(ss => ss.SesionId == id && ss.StudentId == studentId);

        if (link == null)
            return NotFound("Este alumno no está asignado a esta sesión.");

        link.Attended = dto.Attended;
        await _db.SaveChangesAsync();

        return Ok(new { studentId, attended = link.Attended });
    }

    // GET api/sessions/mymysessions — sesiones del usuario logueado
    [HttpGet("mysessions")]
    [Authorize]
    public async Task<IActionResult> GetMySessions()
    {
        // Obtener el userId del token JWT
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var sessions = await _db.Sesions
            .Where(s => s.SesionStudents
                .Any(ss => ss.Student.UserId == userId))
            .Select(s => new SessionDto
            {
                Id = s.Id,
                Date = s.Date,
                Description = s.Description,
                Category = s.Category.ToString(),
                StudentCount = s.SesionStudents.Count
            })
            .OrderByDescending(s => s.Date)
            .ToListAsync();

        return Ok(sessions);
    }


}