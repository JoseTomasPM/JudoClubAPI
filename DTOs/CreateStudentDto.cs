using JudoClubAPI.Models; 

namespace JudoClubAPI.DTOs;


public class CreateStudentDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public Belt Belt { get; set; } = Belt.White;
    public Category Category { get; set; } = Category.Benjamin; 
    public string? PhotoUrl { get; set; }
    public int? UserId { get; set; }
}