using Asp.Versioning;
using GameCatalogue.API;
using GameCatalogue.API.Middleware;
using GameCatalogue.API.Services;
using GameCatalogue.Application;
using GameCatalogue.Application.Interfaces.Storage;
using GameCatalogue.Infrastructure;
using GameCatalogue.Infrastructure.Logging;
using GameCatalogue.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog(SerilogConfiguration.ConfigureSerilog);

// Application & Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Cover image URL resolution (needs the current request to build absolute URLs)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICoverImageUrlResolver, CoverImageUrlResolver>();

// API
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for Angular
builder.Services.AddCors(options =>
    options.AddPolicy("Angular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

// Apply migrations on startup, retrying while the database becomes available.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
    var startupLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("Startup");

    const int maxAttempts = 12;
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await db.Database.MigrateAsync();
            startupLogger.LogInformation("Database migration completed.");
            break;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            startupLogger.LogWarning(
                ex, "Database not ready (attempt {Attempt}/{Max}); retrying in 5s.",
                attempt, maxAttempts);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("Angular");
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

// Machine-readable health (JSON, used by the dashboard page below).
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthResponseWriter.WriteJsonAsync
});

// Human-friendly health dashboard page.
app.MapGet("/health-ui", () => Results.Redirect("/health.html"));

app.Run();

/// <summary>Entry point marker to allow integration testing via WebApplicationFactory.</summary>
public partial class Program { }
