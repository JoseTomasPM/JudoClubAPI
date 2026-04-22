using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JudoClubAPI.Data;
using JudoClubAPI.DTOs;
using JudoClubAPI.Models;

namespace JudoClubAPI.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize(Roles = "Admin")]
public class PaymentStatusController : ControllerBase
{
    private readonly AppDbContext _db;

    public PaymentStatusController(AppDbContext db) => _db = db;

    // PUT api/payments/{id}/status
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdatePaymentStatusDto dto)
    {
        var payment = await _db.Payments.FindAsync(id);
        if (payment == null)
            return NotFound($"No existe ningún pago con Id {id}.");

        payment.Status = dto.Status;
        await _db.SaveChangesAsync();

        return Ok(new PaymentDto
        {
            Id = payment.Id,
            Concept = payment.Concept,
            Amount = payment.Amount,
            Date = payment.Date,
            Status = payment.Status.ToString(),
            StudentId = payment.StudentId
        });
    }

    // GET api/payments/pending
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var payments = await _db.Payments
            .Where(p => p.Status == Status.Pending)
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