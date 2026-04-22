namespace JudoClubAPI.DTOs;

public class StudentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Belt { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; 
    public string? PhotoUrl { get; set; }
    public int UserId { get; set; }
}