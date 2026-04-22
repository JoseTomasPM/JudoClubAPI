namespace JudoClubAPI.Models;


public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public Belt Belt { get; set; }
    public Category Category { get; set; } 
    public string? PhotoUrl { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public List<Payment> Payments { get; set; } = new();
    public List<Document> Documents { get; set; } = new();
    public List<SesionStudent> SesionStudents { get; set; } = new();
}

public enum Belt
{
	// KYU
	White,
	WhiteYellow,
	Yellow,
	YellowOrange,
	Orange,
	OrangeGreen,
	Green,
	GreenBlue,
	Blue,
	BlueBrown,
	Brown,

	// DAN
	Black1Dan,
	Black2Dan,
	Black3Dan,
	Black4Dan,
	Black5Dan,

	RedWhite6Dan,
	RedWhite7Dan,
	RedWhite8Dan,

	Red9Dan,
	Red10Dan
}