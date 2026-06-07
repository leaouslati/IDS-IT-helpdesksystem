using backend.Application.Interfaces;
using backend.Application.Services;
using backend.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services via interfaces
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ILookupService, LookupService>();

// Add JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVue", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:8080")
               .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Auto create database, apply schema additions, and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Add Departments table if it doesn't exist yet
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Departments')
        CREATE TABLE Departments (
            Id int NOT NULL IDENTITY PRIMARY KEY,
            Name nvarchar(max) NOT NULL,
            ManagerId int NULL,
            CONSTRAINT FK_Departments_Users_ManagerId FOREIGN KEY (ManagerId) REFERENCES Users(Id)
        )");

    // Add DepartmentId column to Users if not present
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'DepartmentId')
        ALTER TABLE Users ADD DepartmentId int NULL");

    // Add DepartmentId column to Tickets if not present
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tickets') AND name = 'DepartmentId')
        ALTER TABLE Tickets ADD DepartmentId int NULL");

    // Add AssignedToUserId column to Tickets if not present
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tickets') AND name = 'AssignedToUserId')
        ALTER TABLE Tickets ADD AssignedToUserId int NULL");

    // Add FailedLoginAttempts column to Users if not present
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'FailedLoginAttempts')
        ALTER TABLE Users ADD FailedLoginAttempts int NOT NULL DEFAULT 0");

    // Add LockoutUntil column to Users if not present
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'LockoutUntil')
        ALTER TABLE Users ADD LockoutUntil datetime2 NULL");

    // Add PasswordResetTokens table if not present
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PasswordResetTokens')
        CREATE TABLE PasswordResetTokens (
            Id int NOT NULL IDENTITY PRIMARY KEY,
            UserId int NOT NULL,
            Token nvarchar(100) NOT NULL,
            ExpiresAt datetime2 NOT NULL,
            IsUsed bit NOT NULL DEFAULT 0,
            CreatedAt datetime2 NOT NULL,
            CONSTRAINT FK_PasswordResetTokens_Users_UserId FOREIGN KEY (UserId) REFERENCES Users(Id)
        )");

    // Add escalation tracking columns to Tickets if not present (Part 1 additions)
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tickets') AND name = 'EscalatedByUserId')
        ALTER TABLE Tickets ADD EscalatedByUserId int NULL");

    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tickets') AND name = 'EscalatedAt')
        ALTER TABLE Tickets ADD EscalatedAt datetime2 NULL");

    // Add escalation comment flag to TicketComments if not present (Part 1 additions)
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TicketComments') AND name = 'IsEscalationComment')
        ALTER TABLE TicketComments ADD IsEscalationComment bit NOT NULL DEFAULT 0");

    // Add attachment metadata columns to TicketAttachments if not present (Part 3 additions)
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TicketAttachments') AND name = 'FileSize')
        ALTER TABLE TicketAttachments ADD FileSize bigint NOT NULL DEFAULT 0");

    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TicketAttachments') AND name = 'FileType')
        ALTER TABLE TicketAttachments ADD FileType nvarchar(100) NOT NULL DEFAULT ''");

    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TicketAttachments') AND name = 'UploadedByUserId')
        ALTER TABLE TicketAttachments ADD UploadedByUserId int NOT NULL DEFAULT 0");

    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TicketAttachments') AND name = 'CommentId')
        ALTER TABLE TicketAttachments ADD CommentId int NULL");

    await DbSeeder.SeedAsync(db);
}

// Ensure the file upload directory exists before serving requests
var uploadsDir = Path.Combine(
    app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
    "uploads", "attachments");
Directory.CreateDirectory(uploadsDir);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowVue");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
