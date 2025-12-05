using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MultiAB.Data;
using MultiAB.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // SEGURIDAD: Limitar tamaño de request body (prevenir DoS)
    options.MaxModelBindingCollectionSize = 100; // Máximo 100 items en colecciones
});

// SEGURIDAD: Limitar tamaño de request body
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10485760; // 10 MB máximo
    options.ValueLengthLimit = 10485760; // 10 MB máximo
});

builder.Services.AddEndpointsApiExplorer();

// Swagger solo disponible en modo Development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "MultiAB API",
            Version = "v1",
            Description = "API Multi-tenant para gestión de condominios y casas"
        });
    });
}

// Configurar HttpContextAccessor para TenantProvider
builder.Services.AddHttpContextAccessor();

// Registrar TenantProvider
builder.Services.AddScoped<ITenantProvider, TenantProvider>();

// Registrar PasswordHasher para hashear contraseñas
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// Registrar InputSanitizer para sanitizar entrada
builder.Services.AddScoped<IInputSanitizer, InputSanitizer>();

// Configurar Entity Framework Core con MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Usar ServerVersion.Parse para evitar la detección automática durante el diseño
// MySQL 8.0 es una versión común y compatible
var serverVersion = ServerVersion.Parse("8.0.0-mysql");

// Configurar DbContext con seguridad mejorada
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(connectionString, serverVersion,
        mysqlOptions =>
        {
            mysqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
    
    // SEGURIDAD: Solo habilitar logs sensibles y errores detallados en Development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging(); // Solo en desarrollo
        options.EnableDetailedErrors(); // Solo en desarrollo
    }
    // En Production, estos están deshabilitados por defecto (más seguro)
});

var app = builder.Build();

// Configure the HTTP request pipeline.

// SEGURIDAD: Headers de seguridad (debe ir primero)
app.UseMiddleware<MultiAB.Middleware.SecurityHeadersMiddleware>();

// SEGURIDAD: Manejo de errores (debe ir temprano para capturar todos los errores)
app.UseMiddleware<MultiAB.Middleware.ErrorHandlingMiddleware>();

// SEGURIDAD: Rate limiting básico
app.UseMiddleware<MultiAB.Middleware.RateLimitingMiddleware>();

// Swagger solo disponible en modo Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Solo redirigir a HTTPS si está disponible
// Si HTTPS no funciona, comentar esta línea
// app.UseHttpsRedirection();

// Middleware para establecer el tenant antes de cada request
app.UseMiddleware<MultiAB.Middleware.TenantMiddleware>();

app.UseAuthorization();

// Endpoint raíz - redirigir a Swagger solo en Development
app.MapGet("/", () => 
{
    if (app.Environment.IsDevelopment())
    {
        return Results.Redirect("/swagger");
    }
    return Results.Ok(new { Message = "MultiAB API", Status = "Running" });
});

// Endpoint de información sobre EF Core - Solo en Development
if (app.Environment.IsDevelopment())
{
    app.MapGet("/efcore", async (ApplicationDbContext context) =>
    {
        // SEGURIDAD: No exponer información del sistema
        // Solo información básica sin detalles técnicos
        return Results.Ok(new
        {
            Message = "API funcionando correctamente",
            Status = "OK",
            Endpoints = new
            {
                Swagger = "/swagger",
                Controllers = new
                {
                    Users = "/api/Users",
                    Roles = "/api/Roles",
                    UserRoles = "/api/UserRoles",
                    Condominios = "/api/Condominios",
                    Casas = "/api/Casas",
                    AuditLogs = "/api/AuditLogs"
                }
            }
        });
    });
}

app.MapControllers();

app.Run();
