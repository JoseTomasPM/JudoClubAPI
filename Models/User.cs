namespace JudoClubAPI.Models;


public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }

    public Rol Rol { get; set; }

    public List<Student> Students { get; set; }
}


public enum Rol
{
    Admin,
    User
}