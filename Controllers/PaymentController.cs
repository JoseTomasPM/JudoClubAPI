using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JudoClubAPI.Data;
using JudoClubAPI.DTOs;
using JudoClubAPI.Models;

namespace JudoClubAPI.Controllers;

[ApiController]
[Route("api/students/{studentId}/payments")]
[Authorize(Roles = "Admin")]
public class PaymentController : ControllerBase
{
    private readonly AppDbContext _db;

    public PaymentController(AppDbContext db) => _db = db;

    // GET api/students/{studentId}/payments
    [HttpGet]
    public async Task<IActionResult> GetPayments(int studentId)
    {
        var student = await _db.Students.FindAsync(studentId);
        if (student == null)
            return NotFound($"No existe ningún alumno con Id {studentId}.");

        var payments = await _db.Payments
            .Where(p => p.StudentId == studentId)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                Concept = p.Concept,
                Amount = p.Amount,
                Date = p.Date,
                Status = p.Status.ToString(),
                StudentId = p.StudentId
            })
            .ToListAsync();

        return Ok(payments);
    }

    // POST api/students/{studentId}/payments
    [HttpPost]
    public async Task<IActionResult> CreatePayment(int studentId, CreatePaymentDto dto)
    {
        var student = await _db.Students.FindAsync(studentId);
        if (student == null)
            return NotFound($"No existe ningún alumno con Id {studentId}.");

        var payment = new Payment
        {
            StudentId = studentId,
            Concept = dto.Concept,
            Amount = dto.Amount,
            Date = dto.Date,
            Status = Status.Pending
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        return StatusCode(201, new PaymentDto
        {
            Id = payment.Id,
            Concept = payment.Concept,
            Amount = payment.Amount,
            Date = payment.Date,
            Status = payment.Status.ToString(),
            StudentId = payment.StudentId
        });
    }

    // DELETE api/students/{studentId}/payments/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePayment(int studentId, int id)
    {
        var payment = await _db.Payments
            .FirstOrDefaultAsync(p => p.Id == id && p.StudentId == studentId);
        if (payment == null)
            return NotFound($"No existe ningún pago con Id {id}.");

        _db.Payments.Remove(payment);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // GET api/payments
    // Todos los pagos de todos los alumnos (solo Admin)
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var payments = await _db.Payments
            .Include(p => p.Student)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                Concept = p.Concept,
                Amount = p.Amount,
                Date = p.Date,
                Status = p.Status.ToString(),
                StudentId = p.StudentId
            })
            .ToListAsync();

        return Ok(payments);
    }
}