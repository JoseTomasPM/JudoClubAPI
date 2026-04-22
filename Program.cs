using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using JudoClubAPI.Data;
using System.Text;
using JudoClubAPI.Models;
using Microsoft.AspNetCore.Http.Features;

Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

var builder = WebApplication.CreateBuilder(args);

//  Controllers 
builder.Services.AddControllers();
builder.Services.Configure<FormOptions>(opt =>
{
    opt.MultipartBodyLengthLimit = 5 * 1024 * 1024; // 5MB máximo global
});

//  Database 
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//  JWT Authentication 
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            RoleClaimType = ClaimTypes.Role

        };
        opt.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("JWT ERROR FULL:");
                Console.WriteLine(context.Exception.ToString());
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

//  Swagger 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "JudoClub API", Version = "v1" });

    // Permite enviar el token JWT desde Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce SOLO el token JWT sin la palabra Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            },
            Scheme = "bearer",
            Name = "Authorization",
            In = ParameterLocation.Header
        },
        new List<string>()
    }
    });
});

// CORS 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("https://jolly-bonbon-9822d5.netlify.app")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});




builder.Services.AddHttpContextAccessor();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);







var app = builder.Build();

// DB test 
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.OpenConnection();
    db.Database.CloseConnection();
    Console.WriteLine("?? Connected to DB");
}

//  Middleware 
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Para servir archivos (los PDFs subidos)
app.UseStaticFiles();


/*  CREATE ADMIN
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!db.Users.Any(u => u.Rol == Rol.Admin))
    {
        db.Users.Add(new User
        {
            Email = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
            Rol = Rol.Admin
        });

        db.SaveChanges();
    }
}
*/




//crear usuarios, alumnos, sesiones y pagos de prueba para desarrollo (solo en dev, no en prod)


//if (app.Environment.IsDevelopment())
//{
//    app.MapPost("/api/seed", async (AppDbContext db) =>
//    {
        

//        // ── Usuarios ──────────────────────────
//        var admin = new User
//        {
//            Email = "admin@judoclub.com",
//            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin1234"),
//            Rol = Rol.Admin
//        };
//        var user1 = new User
//        {
//            Email = "padre1@gmail.com",
//            PasswordHash = BCrypt.Net.BCrypt.HashPassword("User1234"),
//            Rol = Rol.User
//        };
//        var user2 = new User
//        {
//            Email = "padre2@gmail.com",
//            PasswordHash = BCrypt.Net.BCrypt.HashPassword("User1234"),
//            Rol = Rol.User
//        };

//        db.Users.AddRange(admin, user1, user2);
//        await db.SaveChangesAsync();

//        // ── Alumnos ───────────────────────────
//        var alumnos = new List<Student>
//        {
//            new() { Name = "Carlos López",    BirthDate = DateTime.UtcNow.AddYears(-10), Belt = Belt.Yellow,  Category = Category.Benjamin, UserId = user1.Id },
//            new() { Name = "María García",    BirthDate = DateTime.UtcNow.AddYears(-12), Belt = Belt.Orange,  Category = Category.Alevin,   UserId = user1.Id },
//            new() { Name = "Pablo Martínez",  BirthDate = DateTime.UtcNow.AddYears(-14), Belt = Belt.Green,   Category = Category.Infantil, UserId = user2.Id },
//            new() { Name = "Laura Sánchez",   BirthDate = DateTime.UtcNow.AddYears(-16), Belt = Belt.Blue,    Category = Category.Cadete,   UserId = user2.Id },
//            new() { Name = "Diego Fernández", BirthDate = DateTime.UtcNow.AddYears(-19), Belt = Belt.Brown,   Category = Category.Junior,   UserId = user1.Id },
//            new() { Name = "Ana Ruiz",        BirthDate = DateTime.UtcNow.AddYears(-10), Belt = Belt.White,   Category = Category.Benjamin, UserId = user2.Id },
//            new() { Name = "Javier Torres",   BirthDate = DateTime.UtcNow.AddYears(-13), Belt = Belt.Yellow,  Category = Category.Alevin,   UserId = user1.Id },
//            new() { Name = "Sofía Moreno",    BirthDate = DateTime.UtcNow.AddYears(-25), Belt = Belt.Black1Dan, Category = Category.Senior, UserId = user2.Id },
//        };

//        db.Students.AddRange(alumnos);
//        await db.SaveChangesAsync();

//        // ── Sesiones ──────────────────────────
//        var sesiones = new List<Sesion>
//        {
//            new() { Date = DateTime.UtcNow.AddDays(-14), Description = "Entrenamiento técnica", Category = Category.Benjamin },
//            new() { Date = DateTime.UtcNow.AddDays(-7),  Description = "Sparring controlado",   Category = Category.Alevin   },
//            new() { Date = DateTime.UtcNow.AddDays(-3),  Description = "Preparación competición", Category = Category.Infantil },
//            new() { Date = DateTime.UtcNow.AddDays(2),   Description = "Entrenamiento físico",   Category = Category.Cadete  },
//            new() { Date = DateTime.UtcNow.AddDays(7),   Description = "Técnicas de suelo",      Category = Category.Junior  },
//            new() { Date = DateTime.UtcNow.AddDays(14),  Description = "Entrenamiento general",  Category = Category.Senior  },
//        };

//        db.Sesions.AddRange(sesiones);
//        await db.SaveChangesAsync();

//        // ── Asignar alumnos a sesiones por categoría ──
//        foreach (var sesion in sesiones)
//        {
//            var alumnosDeSesion = alumnos.Where(a => a.Category == sesion.Category);
//            foreach (var alumno in alumnosDeSesion)
//            {
//                db.SesionStudents.Add(new SesionStudent
//                {
//                    SesionId = sesion.Id,
//                    StudentId = alumno.Id,
//                    Attended = sesion.Date < DateTime.UtcNow // Pasadas: marcar como asistidos
//                });
//            }
//        }

//        // ── Pagos ─────────────────────────────
//        var conceptos = new[] { "Cuota enero", "Cuota febrero", "Cuota marzo", "Material" };
//        var random = new Random();

//        foreach (var alumno in alumnos)
//        {
//            foreach (var concepto in conceptos)
//            {
//                db.Payments.Add(new Payment
//                {
//                    StudentId = alumno.Id,
//                    Concept = concepto,
//                    Amount = concepto == "Material" ? 45.00m : 35.00m,
//                    Date = DateTime.UtcNow.AddMonths(-random.Next(0, 4)),
//                    Status = random.Next(3) switch { 0 => Status.Pending, 1 => Status.Completed, _ => Status.Completed }
//                });
//            }
//        }

//        await db.SaveChangesAsync();

//        return Results.Ok(new
//        {
//            mensaje = "✅ Seed completado",
//            usuarios = 3,
//            alumnos = alumnos.Count,
//            sesiones = sesiones.Count,
//            pagos = alumnos.Count * conceptos.Length
//        });
//    }).AllowAnonymous();
//}





app.Run();