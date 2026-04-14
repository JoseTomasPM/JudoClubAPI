namespace JudoClubAPI.Models;

public class SesionStudent
{
	public int SesionId { get; set; }
	public Sesion Sesion { get; set; }

	public int StudentId { get; set; }
	public Student Student { get; set; }
}