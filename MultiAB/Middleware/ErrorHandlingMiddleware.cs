using System.Net;
using System.Text.Json;

namespace MultiAB.Middleware;

/// <summary>
/// Middleware para manejar errores de forma segura sin exponer información sensible
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ErrorHandlingMiddleware(
        RequestDelegate next, 
        ILogger<ErrorHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        // Log del error (con información completa para el desarrollador)
        _logger.LogError(exception, "Error no manejado: {Message}", exception.Message);

        // Respuesta al cliente (sin información sensible)
        object response;
        if (_environment.IsDevelopment())
        {
            response = new
            {
                error = "Ha ocurrido un error",
                message = exception.Message,
                details = exception.StackTrace
            };
        }
        else
        {
            response = new
            {
                error = "Ha ocurrido un error al procesar la solicitud",
                message = "Por favor, intenta más tarde"
            };
        }

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var jsonResponse = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(jsonResponse);
    }
}

