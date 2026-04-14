namespace JudoClubAPI.Models;

public class Payment
{
    public int Id { get; set; }
    public Student Student { get; set; }
    public DateTime Date { get; set; }
    public string Concept { get; set; }
    public int StudentId { get; set; }
    public decimal Amount { get; set; }
    public Status Status { get; set; }


}

public enum Status
{
    Pending,
    Completed,
    Failed
}   