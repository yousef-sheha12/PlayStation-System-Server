using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PlayStation.API.Middleware;
using PlayStation.Application.Interfaces;
using PlayStation.Application.Mappings;
using PlayStation.Application.Validators;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PlayStation.Infrastructure.Data;
using PlayStation.Infrastructure.Repositories;
using PlayStation.Infrastructure.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/ps-system-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiResponseFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PlayStation Management System API",
        Version = "v1",
        Description = "A complete PlayStation Management System Backend API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<PlayStationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PlayStation.Application.Features.Products.Commands.CreateProductCommand).Assembly));

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

builder.Services.AddValidatorsFromAssembly(typeof(CreateProductValidator).Assembly);

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
});

app.UseMiddleware<GlobalExceptionMiddleware>();

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PlayStation Management System API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    name = "PlayStation Management System API",
    version = "v1",
    status = "running",
    swagger = app.Environment.IsProduction() ? null : "/swagger",
    endpoints = new[]
    {
        "POST /api/auth/login",
        "POST /api/auth/register",
        "GET /api/dashboard",
        "GET /api/sessions",
        "GET /api/products",
        "GET /api/invoices",
        "GET /api/reports",
        "GET /api/devices",
        "GET /health"
    }
}));

app.MapGet("/health", async (PlayStationDbContext context) =>
{
    try
    {
        await context.Database.CanConnectAsync();
        return Results.Ok(new { status = "healthy", database = "connected" });
    }
    catch
    {
        return Results.Ok(new { status = "unhealthy", database = "disconnected" });
    }
});

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PlayStationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var pendingMigrations = context.Database.GetPendingMigrations().ToList();
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Applying {Count} pending migrations", pendingMigrations.Count);
            context.Database.Migrate();
        }
        else
        {
            logger.LogInformation("No pending migrations");
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Migration failed. Falling back to EnsureCreated.");
        try { context.Database.EnsureCreated(); } catch (Exception ex2) { logger.LogError(ex2, "EnsureCreated failed"); }
    }

    try
    {
        context.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Sessions' AND COLUMN_NAME='CustomerName')
                ALTER TABLE Sessions ADD CustomerName nvarchar(200) NULL;

            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Sessions' AND COLUMN_NAME='HourlyRate')
                ALTER TABLE Sessions ADD HourlyRate decimal(18,2) NOT NULL DEFAULT 0;
        ");
        logger.LogInformation("Ensured Session columns exist (CustomerName, HourlyRate)");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not add missing columns (they may already exist)");
    }

    try
    {
        if (!context.Roles.Any(r => r.Name == "Worker"))
        {
            context.Roles.Add(new PlayStation.Domain.Entities.Role
            {
                Name = "Worker",
                Description = "System Worker",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
            context.SaveChanges();
        }

        var workerRole = context.Roles.FirstOrDefault(r => r.Name == "Worker");
        if (workerRole != null)
        {
            var workerUser = context.Users.FirstOrDefault(u => u.Email == "worker@playstation.com");
            if (workerUser == null)
            {
                context.Users.Add(new PlayStation.Domain.Entities.User
                {
                    Email = "worker@playstation.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Worker@123"),
                    FullName = "PlayStation Worker",
                    RoleId = workerRole.Id,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });
                context.SaveChanges();
            }
            else
            {
                workerUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Worker@123");
                workerUser.FullName = "PlayStation Worker";
                workerUser.IsActive = true;
                context.SaveChanges();
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to seed database");
    }
}

app.Run();
