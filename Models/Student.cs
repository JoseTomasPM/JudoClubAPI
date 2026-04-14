namespace JudoClubAPI.Models;


public class Student
{
	public int Id { get; set; }
	public string Name { get; set; }
	public DateTime BirthDate { get; set; }
	public Belt Belt { get; set; }
	public string Category { get; set; }
	public string? PhotoUrl { get; set; }
	public int UserId { get; set; }
	public User User { get; set; }
	public List<Payment> Payments { get; set; }
	public List<Document> Documents { get; set; }
	public List<SesionStudent> SesionStudents { get; set; }
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