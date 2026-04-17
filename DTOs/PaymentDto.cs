namespace JudoClubAPI.DTOs;

public class PaymentDto
{
    public int Id { get; set; }
    public string Concept { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public int StudentId { get; set; }
}