using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ProjectControlsReportingTool.API.Data;
using ProjectControlsReportingTool.API.Business.AppSettings;
using ProjectControlsReportingTool.API.Repositories.Interfaces;
using ProjectControlsReportingTool.API.Repositories.Base;
using ProjectControlsReportingTool.API.Repositories.Implementations;
using ProjectControlsReportingTool.API.Repositories;
using ProjectControlsReportingTool.API.Business.Interfaces;
using RazorLight;
using ProjectControlsReportingTool.API.Business.Services;
using ProjectControlsReportingTool.API.Middleware;
using ProjectControlsReportingTool.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Configure AppSettings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<FileStorageSettings>(builder.Configuration.GetSection("FileStorage"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<PushNotificationOptions>(builder.Configuration.GetSection("PushNotificationSettings"));

// Configure Caching - Use in-memory cache for now (Redis can be configured later)
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<Microsoft.Extensions.Caching.Distributed.IDistributedCache, Microsoft.Extensions.Caching.Distributed.MemoryDistributedCache>();

// Configure dependency injection
void SetupDependencyInjection(WebApplicationBuilder webApplicationBuilder)
{
    // Repository pattern dependencies
    webApplicationBuilder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
    
    // Specific repositories
    webApplicationBuilder.Services.AddScoped<IUserRepository, UserRepository>();
    webApplicationBuilder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
    webApplicationBuilder.Services.AddScoped<IReportRepository, ReportRepository>();
    webApplicationBuilder.Services.AddScoped<IReportTemplateRepository, ReportTemplateRepository>();
    
    // Business services
    webApplicationBuilder.Services.AddScoped<IUserService, UserService>();
    webApplicationBuilder.Services.AddScoped<IReportService, ReportService>();
    webApplicationBuilder.Services.AddScoped<IAuthService, AuthService>();
    webApplicationBuilder.Services.AddScoped<IReportTemplateService, ReportTemplateService>();
    webApplicationBuilder.Services.AddScoped<IExportService, ExportService>();
    
    // Phase 9: Advanced Features
    webApplicationBuilder.Services.AddScoped<IComplianceService, ComplianceService_Simple>();
    webApplicationBuilder.Services.AddScoped<IWebhookService, WebhookService_Simple>();
    webApplicationBuilder.Services.AddScoped<ICacheService, CacheService>();
    
    // Analytics data access service
    webApplicationBuilder.Services.AddScoped<IAnalyticsDataAccessService, AnalyticsDataAccessService>();
    
    // Email service
    webApplicationBuilder.Services.AddScoped<IEmailService, EmailService>();
    
    // Real-time notification service
    webApplicationBuilder.Services.AddScoped<IRealTimeNotificationService, RealTimeNotificationService>();
    
    // Phase 11.3: User Notification Preferences
    webApplicationBuilder.Services.AddScoped<IUserNotificationPreferenceService, UserNotificationPreferenceService>();
    webApplicationBuilder.Services.AddScoped<IUserNotificationPreferenceRepository, UserNotificationPreferenceRepository>();
    
    // Phase 11.3: Email Template Management
    webApplicationBuilder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
    webApplicationBuilder.Services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
    
    // Phase 11.3: Push Notifications
    webApplicationBuilder.Services.AddScoped<IPushNotificationService, PushNotificationService>();
    
    // RazorLight for email template rendering
    webApplicationBuilder.Services.AddSingleton<IRazorLightEngine>(provider =>
    {
        return new RazorLightEngineBuilder()
            .UseMemoryCachingProvider()
            .Build();
    });
}

// Add services to the container
builder.Services.AddControllers();

// Add SignalR for real-time notifications
builder.Services.AddSignalR();

// Add HttpClient for webhook service
builder.Services.AddHttpClient();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Add Authorization
builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200") // Angular dev server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for SignalR
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Project Controls Reporting Tool API", 
        Version = "v1",
        Description = "API for Project Controls Reporting Tool - A streamlined workflow-based reporting system"
    });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Setup dependency injection
SetupDependencyInjection(builder);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Project Controls Reporting Tool API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();

// Add security middleware
app.UseMiddleware<SecurityMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

app.UseCors("DefaultPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR Hub
app.MapHub<NotificationHub>("/hubs/notifications");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        context.Database.EnsureCreated();
    }
    catch (Exception)
    {
        // Database creation failed - continue without error for now
    }
}

app.Run();

// Make Program class accessible for testing
public partial class Program 
{
    protected Program() { }
}

