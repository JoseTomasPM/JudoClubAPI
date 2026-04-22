namespace JudoClubAPI.Models;


public class Sesion
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public Category Category { get; set; } 
    public List<SesionStudent> SesionStudents { get; set; } = new();
}