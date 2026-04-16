using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JudoClubAPI.Data;
using JudoClubAPI.DTOs;
using JudoClubAPI.Helpers;
using JudoClubAPI.Models;

namespace JudoClubAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentController : ControllerBase
{
    private readonly AppDbContext _db;

    public StudentController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/student
    // Admin -> todos los alumnos
    // User  -> solo los suyos
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var myId = UserHelper.GetUserId(User);

        var query = UserHelper.IsAdmin(User)
            ? _db.Students                                  // Admin: todos
            : _db.Students.Where(s => s.UserId == myId);   // User: solo los suyos

        var students = await query
            .Select(s => new StudentDto
            {
                Id = s.Id,
                Name = s.Name,
                BirthDate = s.BirthDate,
                Belt = s.Belt.ToString(),
                Category = s.Category,
                PhotoUrl = s.PhotoUrl,
                UserId = s.UserId
            })
            .ToListAsync();

        return Ok(students);
    }
    
    // GET api/student{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var myId = UserHelper.GetUserId(User);
        var isAdmin = UserHelper.IsAdmin(User);

        // 1. Construyes la query base con control de acceso
        var query = _db.Students.AsQueryable();

        if (!isAdmin)
        {
            query = query.Where(s => s.UserId == myId);
        }

        // 2. Aplicas el filtro del ID SIEMPRE después del control de acceso
        var student = await query
            .Where(s => s.Id == id)
            .Select(s => new StudentDto
            {
                Id = s.Id,
                Name = s.Name,
                BirthDate = s.BirthDate,
                Belt = s.Belt.ToString(),
                Category = s.Category,
                PhotoUrl = s.PhotoUrl,
                UserId = s.UserId
            })
            .FirstOrDefaultAsync();

        // 3. Si no existe (o no tienes acceso, cae aquí igual)
        if (student == null)
            return NotFound($"No existe ningún alumno con Id {id}.");

        return Ok(student);
    }

    // PUT api/student{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateStudentDto dto)
    {
        var myId = UserHelper.GetUserId(User);
        var isAdmin = UserHelper.IsAdmin(User);
        // 1. Construyes la query base con control de acceso
        var query = _db.Students.AsQueryable();
        if (!isAdmin)
        {
            query = query.Where(s => s.UserId == myId);
        }
        // 2. Aplicas el filtro del ID SIEMPRE después del control de acceso
        var student = await query.FirstOrDefaultAsync(s => s.Id == id);
        // 3. Si no existe (o no tienes acceso, cae aquí igual)
        if (student == null)
            return NotFound($"No existe ningún alumno con Id {id}.");
        // Solo actualizamos los campos editables, el UserId no se puede cambiar aquí
        student.Name = dto.Name;
        student.BirthDate = dto.BirthDate;
        student.Belt = dto.Belt;
        student.Category = dto.Category;
        student.PhotoUrl = dto.PhotoUrl;
        await _db.SaveChangesAsync();
        return Ok(new StudentDto
        {
            Id = student.Id,
            Name = student.Name,
            BirthDate = student.BirthDate,
            Belt = student.Belt.ToString(),
            Category = student.Category,
            PhotoUrl = student.PhotoUrl,
            UserId = student.UserId
        });
    }


    //DELETE api/student{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var student = await _db.Students.FindAsync(id);

        if (student == null)
            return NotFound($"No existe ningún alumno con Id {id}.");

        _db.Students.Remove(student);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // POST api/student
    [HttpPost]
    public async Task<IActionResult> Create(CreateStudentDto dto)
    {
        var myId = UserHelper.GetUserId(User);

        int assignedUserId;

        if (UserHelper.IsAdmin(User))
        {
            // Admin debe especificar a qué usuario pertenece el alumno
            if (dto.UserId is null)
                return BadRequest("El admin debe especificar el UserId del tutor.");

            // Verificar que ese usuario existe
            var userExists = await _db.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists)
                return NotFound($"No existe ningún usuario con Id {dto.UserId}.");

            assignedUserId = dto.UserId.Value;
        }
        else
        {
            // Usuario normal → el alumno siempre es suyo, ignoramos dto.UserId
            assignedUserId = myId;
        }

        var student = new Student
        {
            Name = dto.Name,
            BirthDate = dto.BirthDate,
            Belt = dto.Belt,
            Category = dto.Category,
            PhotoUrl = dto.PhotoUrl,
            UserId = assignedUserId
        };

        _db.Students.Add(student);
        await _db.SaveChangesAsync();

        return StatusCode(201, new StudentDto
        {
            Id = student.Id,
            Name = student.Name,
            BirthDate = student.BirthDate,
            Belt = student.Belt.ToString(),
            Category = student.Category,
            PhotoUrl = student.PhotoUrl,
            UserId = student.UserId
        });
    }

    //GET api/student/mine
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyStudents()
    {
        var myId = UserHelper.GetUserId(User);
        var students = await _db.Students
            .Where(s => s.UserId == myId)
            .Select(s => new StudentDto
            {
                Id = s.Id,
                Name = s.Name,
                BirthDate = s.BirthDate,
                Belt = s.Belt.ToString(),
                Category = s.Category,
                PhotoUrl = s.PhotoUrl,
                UserId = s.UserId
            })
            .ToListAsync();
        return Ok(students);
    }
}