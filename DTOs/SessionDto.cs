using JudoClubAPI.Models;
namespace JudoClubAPI.DTOs;


public class SessionDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // sigue string — se convierte con .ToString()
    public int StudentCount { get; set; }
}

public class CreateSessionDto
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public Category Category { get; set; } = Category.Benjamin; 
}

// Alumno simplificado para mostrar dentro de una sesión
public class SessionStudentDto
{
    public int StudentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Belt { get; set; } = string.Empty;
    public bool Attended { get; set; }
}