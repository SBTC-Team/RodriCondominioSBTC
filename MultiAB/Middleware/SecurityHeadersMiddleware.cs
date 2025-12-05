namespace MultiAB.Middleware;

/// <summary>
/// Middleware para agregar headers de seguridad HTTP
/// Protege contra ataques comunes y oculta información del servidor
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;

    public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Remover headers que exponen información del servidor
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");
        context.Response.Headers.Remove("X-AspNet-Version");
        context.Response.Headers.Remove("X-AspNetMvc-Version");

        // Headers de seguridad (usar indexer para evitar duplicados)
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        
        // Content Security Policy: más permisivo para Swagger en Development
        var isSwagger = context.Request.Path.StartsWithSegments("/swagger");
        if (isSwagger)
        {
            // CSP permisivo para Swagger UI (permite inline scripts y styles necesarios)
            context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:;";
        }
        else
        {
            // CSP estricto para el resto de la aplicación
            context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";
        }
        
        // No exponer información del servidor
        context.Response.Headers["X-Robots-Tag"] = "noindex, nofollow";

        await _next(context);
    }
}

