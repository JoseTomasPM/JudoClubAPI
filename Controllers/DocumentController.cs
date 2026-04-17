using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JudoClubAPI.Data;
using JudoClubAPI.DTOs;
using JudoClubAPI.Helpers;
using JudoClubAPI.Models;

namespace JudoClubAPI.Controllers;

[ApiController]
[Route("api/students/{studentId}/documents")]
[Authorize]
public class DocumentController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public DocumentController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // POST api/students/{studentId}/documents
    // Admin y User pueden subir — UploadBy se marca según el rol
    [HttpPost]
    public async Task<IActionResult> Upload(int studentId, IFormFile file)
    {
        // 1. Verificar que el alumno existe
        var student = await _db.Students.FindAsync(studentId);
        if (student == null)
            return NotFound($"No existe ningún alumno con Id {studentId}.");

        // 2. Control de acceso — User solo puede subir a sus alumnos
        var myId = UserHelper.GetUserId(User);
        if (!UserHelper.IsAdmin(User) && student.UserId != myId)
            return Forbid();

        // 3. Validaciones del archivo
        if (file == null || file.Length == 0)
            return BadRequest("No se ha enviado ningún archivo.");

        if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Solo se permiten archivos PDF.");

        // Límite de tamaño: 5MB por archivo
        const long maxSize = 5 * 1024 * 1024;
        if (file.Length > maxSize)
            return BadRequest("El archivo no puede superar los 5MB.");

        // Límite de documentos por alumno: máximo 10
        const int maxDocs = 10;
        var docCount = await _db.Documents.CountAsync(d => d.StudentId == studentId);
        if (docCount >= maxDocs)
            return BadRequest($"Este alumno ya tiene el máximo de {maxDocs} documentos permitidos.");

        // 4. Crear carpeta si no existe
        var folder = Path.Combine(_env.WebRootPath, "documents", studentId.ToString());
        Directory.CreateDirectory(folder);

        // 5. Nombre único para evitar colisiones
        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(folder, fileName);

        // 6. Guardar el archivo en disco
        using (var stream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(stream);

        // 7. Guardar ruta en base de datos
        var relativeUrl = $"/documents/{studentId}/{fileName}";
        var uploadBy = UserHelper.IsAdmin(User) ? UploadBy.Admin : UploadBy.User;

        var doc = new Document
        {
            Name = file.FileName,
            Url = relativeUrl,
            StudentId = studentId,
            UploadDate = DateTime.UtcNow,
            UploadBy = uploadBy
        };

        _db.Documents.Add(doc);
        await _db.SaveChangesAsync();

        return StatusCode(201, new DocumentDto
        {
            Id = doc.Id,
            Name = doc.Name,
            Url = doc.Url,
            UploadBy = doc.UploadBy.ToString(),
            UploadDate = doc.UploadDate,
            StudentId = doc.StudentId
        });
    }


    //GET api/students/{studentId}/documents
    [HttpGet]
    public async Task<IActionResult> GetAll(int studentId)
    {
        // 1. Verificar que el alumno existe
        var student = await _db.Students.FindAsync(studentId);
        if (student == null)
            return NotFound($"No existe ningún alumno con Id {studentId}.");
        // 2. Control de acceso — User solo puede ver a sus alumnos
        var myId = UserHelper.GetUserId(User);
        if (!UserHelper.IsAdmin(User) && student.UserId != myId)
            return Forbid();
        // 3. Obtener documentos del alumno
        var documents = await _db.Documents
            .Where(d => d.StudentId == studentId)
            .Select(d => new DocumentDto
            {
                Id = d.Id,
                Name = d.Name,
                Url = d.Url,
                UploadBy = d.UploadBy.ToString(),
                UploadDate = d.UploadDate,
                StudentId = d.StudentId
            })
            .ToListAsync();
        return Ok(documents);
    }

    //DELETE api/students/{studentId}/documents/{documentId}
    [HttpDelete("{documentId}")]
    public async Task<IActionResult> Delete(int studentId, int documentId)
    {
        // 1. Verificar que el alumno existe
        var student = await _db.Students.FindAsync(studentId);
        if (student == null)
            return NotFound($"No existe ningún alumno con Id {studentId}.");
        // 2. Control de acceso — User solo puede eliminar de sus alumnos
        var myId = UserHelper.GetUserId(User);
        if (!UserHelper.IsAdmin(User) && student.UserId != myId)
            return Forbid();
        // 3. Verificar que el documento existe y pertenece al alumno
        var doc = await _db.Documents.FirstOrDefaultAsync(d => d.Id == documentId && d.StudentId == studentId);
        if (doc == null)
            return NotFound($"No existe ningún documento con Id {documentId} para el alumno {studentId}.");
        // 4. Eliminar archivo del disco
        var filePath = Path.Combine(_env.WebRootPath, doc.Url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);
        // 5. Eliminar registro de la base de datos
        _db.Documents.Remove(doc);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}