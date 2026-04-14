using Microsoft.EntityFrameworkCore;
using JudoClubAPI.Models;

namespace JudoClubAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Sesion> Sesions => Set<Sesion>();
    public DbSet<SesionStudent> SesionStudents => Set<SesionStudent>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // User - email �nico
        mb.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
        });

        // Student -> User (un usuario tiene muchos alumnos)
        mb.Entity<Student>(e =>
        {
            e.HasOne(s => s.User)
             .WithMany(u => u.Students)
             .HasForeignKey(s => s.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Payment -> Student
        mb.Entity<Payment>(e =>
        {
            e.Property(p => p.Amount).HasPrecision(18, 2);
            e.HasOne(p => p.Student)
             .WithMany(s => s.Payments)
             .HasForeignKey(p => p.StudentId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Document -> Student
        mb.Entity<Document>(e =>
        {
            e.HasOne(d => d.Student)
             .WithMany(s => s.Documents)
             .HasForeignKey(d => d.StudentId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // SesionStudent -> clave compuesta (tabla intermedia)
        mb.Entity<SesionStudent>(e =>
        {
            e.HasKey(ss => new { ss.SesionId, ss.StudentId });
            e.HasOne(ss => ss.Sesion)
             .WithMany(s => s.SesionStudents)
             .HasForeignKey(ss => ss.SesionId);
            e.HasOne(ss => ss.Student)
             .WithMany(s => s.SesionStudents)
             .HasForeignKey(ss => ss.StudentId);
        });
    }
}