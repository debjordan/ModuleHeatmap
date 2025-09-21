using Microsoft.EntityFrameworkCore;
using ModuleHeatMap.Infrastructure.Data;
using ModuleHeatMap.Infrastructure.Repositories;
using ModuleHeatMap.Application.Services;
using ModuleHeatMap.Application.Validators;
using ModuleHeatMap.Core.Interfaces;
using FluentValidation;
using ModuleHeatMap.Application.DTOs;
using ModuleHeatMap.API.Middleware;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() {
        Title = "ModuleHeatMap API",
        Version = "v1",
        Description = "API para monitoramento de consumo de módulos e analytics de engajamento"
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Server=localhost;Database=ModuleHeatMapDB;Trusted_Connection=true;TrustServerCertificate=true;";

    options.UseSqlServer(connectionString);
});

builder.Services.AddAutoMapper(typeof(ModuleHeatMap.Application.Mappings.AutoMapperProfile));
builder.Services.AddScoped<IValidator<ModuleAccessDto>, ModuleAccessValidator>();
builder.Services.AddScoped<IModuleAccessRepository, ModuleAccessRepository>();
builder.Services.AddScoped<IModuleHeatMapService, ModuleHeatMapService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ModuleHeatMap API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Endpoint de informações da API
app.MapGet("/", () => new
{
    service = "ModuleHeatMap API",
    version = "1.0.0",
    description = "Sistema de monitoramento de consumo de módulos",
    endpoints = new
    {
        swagger = "/swagger",
        health = "/health",
        tracking = "/api/tracking",
        analytics = "/api/analytics"
    }
});

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating database");
    }
}

app.Run();
