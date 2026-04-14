namespace JudoClubAPI.Models;


public class Document
{
	public int Id { get; set; }
	public string Name { get; set; }
	public int StudentId { get; set; }
	public Student Student { get; set; } 
	public string Url { get; set; }
	public DateTime UploadDate { get; set; }
	public UploadBy UploadBy { get; set; }
}

public enum UploadBy
{
	User,
	Admin
}