using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JudoClubAPI.Data;
using JudoClubAPI.DTOs;
using JudoClubAPI.Models;

namespace JudoClubAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuthController(AppDbContext db)
    {
        _db = db;
    }

    // POST api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        // 1. Comprobar que el email no existe ya
        var exists = await _db.Users.AnyAsync(u => u.Email == dto.Email);
        if (exists)
            return Conflict("Existing user");

        // 2. Hashear la contrase�a
        var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        // 3. Crear y guardar el usuario
        var user = new User
        {
            Email = dto.Email,
            PasswordHash = hash,
            Rol = dto.Rol.User
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // 4. Devolver 201 Created
        return StatusCode(201, new { message = "Usuario register.", userId = user.Id });
    }
}