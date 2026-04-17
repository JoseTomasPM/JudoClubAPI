namespace JudoClubAPI.DTOs;

public class CreatePaymentDto
{
    public string Concept { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}