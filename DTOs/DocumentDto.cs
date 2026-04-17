namespace JudoClubAPI.DTOs;

public class DocumentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string UploadBy { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public int StudentId { get; set; }
}