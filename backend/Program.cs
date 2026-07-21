using backend.Application.Common;
using backend.Application.Interfaces;
using backend.Application.MappingProfiles;
using backend.Application.Services;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories;
using backend.Infrastructure.Services;
using backend.Presentation.Hubs;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ── MVC / JSON ────────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        // Prevent circular reference serialization errors from EF navigation properties
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── AutoMapper ────────────────────────────────────────────────────────────────
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(TicketMappingProfile).Assembly));

// ── Groq AI settings + HTTP client ─────────────────────────────────────────────
builder.Services.Configure<GroqSettings>(builder.Configuration.GetSection("GroqSettings"));
builder.Services.AddHttpClient("groq", (sp, client) =>
{
    var groqSettings = sp.GetRequiredService<IOptions<GroqSettings>>().Value;
    client.BaseAddress = new Uri(groqSettings.BaseUrl.TrimEnd('/') + "/");
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", groqSettings.ApiKey);
});

// ── SignalR ───────────────────────────────────────────────────────────────────
builder.Services.AddSignalR();
// Map authenticated user's numeric ID claim to SignalR's user identifier
builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();

// ── Repository registrations ──────────────────────────────────────────────────
builder.Services.AddScoped<ITicketRepository,         TicketRepository>();
builder.Services.AddScoped<ITicketHoursLogRepository, TicketHoursLogRepository>();
builder.Services.AddScoped<IAuthRepository,           AuthRepository>();
builder.Services.AddScoped<ILookupRepository,         LookupRepository>();
builder.Services.AddScoped<IDashboardRepository,      DashboardRepository>();
builder.Services.AddScoped<IAdminRepository,          AdminRepository>();
builder.Services.AddScoped<INotificationRepository,   NotificationRepository>();
builder.Services.AddScoped<IReportRepository,         ReportRepository>();
builder.Services.AddScoped<IProfileRepository,        ProfileRepository>();
builder.Services.AddScoped<IHolidayRepository,        HolidayRepository>();

// ── Service registrations ─────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService,           AuthService>();
builder.Services.AddScoped<IDashboardService,      DashboardService>();
builder.Services.AddScoped<ITicketService,         TicketService>();
builder.Services.AddScoped<ILookupService,         LookupService>();
builder.Services.AddScoped<ITicketHoursLogService, TicketHoursLogService>();
builder.Services.AddScoped<IAdminService,          AdminService>();
builder.Services.AddScoped<INotificationService,   NotificationService>();
builder.Services.AddScoped<IFileStorageService,    FileStorageService>();
builder.Services.AddSingleton<IEmailService,       EmailService>();
builder.Services.AddScoped<IReportService,         ReportService>();
builder.Services.AddScoped<IPdfReportService,      PdfReportService>();
builder.Services.AddScoped<IExcelReportService,    ExcelReportService>();
builder.Services.AddScoped<IProfileService,        ProfileService>();
builder.Services.AddScoped<IHolidayService,        HolidayService>();
builder.Services.AddScoped<IGroqService,           GroqService>();

// ── JWT Authentication ────────────────────────────────────────────────────────
// ── QuestPDF community license ────────────────────────────────────────────────
QuestPDF.Settings.License = LicenseType.Community;

var jwtKey      = builder.Configuration["Jwt:Key"]!;
var jwtIssuer   = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtIssuer,
            ValidAudience            = jwtAudience,
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };

        // Allow SignalR to pass the JWT token via query string (WebSocket cannot set headers)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path        = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

// ── CORS ──────────────────────────────────────────────────────────────────────
// AllowCredentials() is required for SignalR WebSocket/SSE transport cross-origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVue", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:8080")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ── Schema bootstrap (EnsureCreated + additive column migrations) ─────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Departments')
        CREATE TABLE Departments (
            Id int NOT NULL IDENTITY PRIMARY KEY,
            Name nvarchar(max) NOT NULL,
            ManagerId int NULL,
            CONSTRAINT FK_Departments_Users_ManagerId FOREIGN KEY (ManagerId) REFERENCES Users(Id)
        )");

    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'DepartmentId')
        ALTER TABLE Users ADD DepartmentId int NULL");

    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tickets') AND name = 'DepartmentId')
        ALTER TABLE Tickets ADD DepartmentId int NULL");

    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tickets') AND name = 'AssignedToUserId')
        ALTER TABLE Tickets ADD AssignedToUserId int NULL");

    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'FailedLoginAttempts')
        ALTER TABLE Users ADD FailedLoginAttempts int NOT NULL DEFAULT 0");

    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'LockoutUntil')
        ALTER TABLE Users ADD LockoutUntil datetime2 NULL");

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

    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PasswordResetTokens') AND name = 'OtpHash')
        ALTER TABLE PasswordResetTokens ADD OtpHash nvarchar(200) NOT NULL DEFAULT ''");

    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PasswordResetTokens') AND name = 'OtpExpiresAt')
        ALTER TABLE PasswordResetTokens ADD OtpExpiresAt datetime2 NOT NULL DEFAULT '1900-01-01'");

    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PasswordResetTokens') AND name = 'OtpAttempts')
        ALTER TABLE PasswordResetTokens ADD OtpAttempts int NOT NULL DEFAULT 0");

    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PasswordResetTokens') AND name = 'IsOtpVerified')
        ALTER TABLE PasswordResetTokens ADD IsOtpVerified bit NOT NULL DEFAULT 0");

    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tickets') AND name = 'EscalatedByUserId')
        ALTER TABLE Tickets ADD EscalatedByUserId int NULL");

    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tickets') AND name = 'EscalatedAt')
        ALTER TABLE Tickets ADD EscalatedAt datetime2 NULL");

    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TicketComments') AND name = 'IsEscalationComment')
        ALTER TABLE TicketComments ADD IsEscalationComment bit NOT NULL DEFAULT 0");

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

    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ActivityLogs') AND name = 'TicketId')
        ALTER TABLE ActivityLogs ADD TicketId int NULL");

    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ActivityLogs') AND name = 'FromValue')
        ALTER TABLE ActivityLogs ADD FromValue nvarchar(200) NULL");

    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ActivityLogs') AND name = 'ToValue')
        ALTER TABLE ActivityLogs ADD ToValue nvarchar(200) NULL");

    // TicketHoursLogs table for agent hours tracking
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TicketHoursLogs')
        CREATE TABLE TicketHoursLogs (
            TicketHoursLogId int NOT NULL IDENTITY PRIMARY KEY,
            TicketId int NOT NULL,
            AgentId int NOT NULL,
            HoursWorked decimal(8,2) NOT NULL,
            LogDate datetime2 NOT NULL,
            Notes nvarchar(max) NULL,
            CONSTRAINT FK_TicketHoursLogs_Tickets_TicketId FOREIGN KEY (TicketId) REFERENCES Tickets(Id) ON DELETE CASCADE,
            CONSTRAINT FK_TicketHoursLogs_Users_AgentId FOREIGN KEY (AgentId) REFERENCES Users(Id)
        )");

    // ── Week 5 schema additions ────────────────────────────────────────────

    // Notification: add TicketId, Type, Title columns
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Notifications') AND name = 'TicketId')
        ALTER TABLE Notifications ADD TicketId int NULL");

    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Notifications') AND name = 'Type')
        ALTER TABLE Notifications ADD Type nvarchar(100) NOT NULL DEFAULT ''");

    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Notifications') AND name = 'Title')
        ALTER TABLE Notifications ADD Title nvarchar(200) NOT NULL DEFAULT ''");

    // TicketAttachment: add StoredFileName for new file storage scheme
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TicketAttachments') AND name = 'StoredFileName')
        ALTER TABLE TicketAttachments ADD StoredFileName nvarchar(300) NOT NULL DEFAULT ''");

    // TicketComments: add IsAttachmentOnly for system-generated timeline entries
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TicketComments') AND name = 'IsAttachmentOnly')
        ALTER TABLE TicketComments ADD IsAttachmentOnly bit NOT NULL DEFAULT 0");

    // ── Week 6 schema additions ────────────────────────────────────────────

    // TicketComments: internal notes visible to Agent/Admin only
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TicketComments') AND name = 'IsInternal')
        ALTER TABLE TicketComments ADD IsInternal bit NOT NULL DEFAULT 0");

    // Users: profile picture path
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'ProfilePictureUrl')
        ALTER TABLE Users ADD ProfilePictureUrl nvarchar(300) NULL");

    // Holidays: admin-managed holiday calendar
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Holidays')
        CREATE TABLE Holidays (
            Id int NOT NULL IDENTITY PRIMARY KEY,
            Name nvarchar(100) NOT NULL,
            Date datetime2 NOT NULL,
            IsRecurring bit NOT NULL DEFAULT 0,
            CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE()
        )");

    await DbSeeder.SeedAsync(db);
}

// ── Upload directory (legacy wwwroot location still served for existing files) ──
var uploadsDir = Path.Combine(
    app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
    "uploads", "attachments");
Directory.CreateDirectory(uploadsDir);

// ── App_Data upload directory (new protected storage) ────────────────────────
var appDataDir = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "uploads", "tickets");
Directory.CreateDirectory(appDataDir);

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

// ── SignalR hub ───────────────────────────────────────────────────────────────
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
