using System.Collections.Concurrent;

namespace MultiAB.Middleware;

/// <summary>
/// Middleware básico de rate limiting para proteger contra DDoS y fuerza bruta
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    
    // Diccionario para almacenar contadores por IP
    private static readonly ConcurrentDictionary<string, RateLimitInfo> _requestCounts = new();
    
    // Configuración: máximo 100 peticiones por minuto por IP
    private const int MaxRequestsPerMinute = 100;
    private const int TimeWindowSeconds = 60;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Obtener IP del cliente (sin exponerla en la respuesta)
        var clientIp = GetClientIpAddress(context);
        
        if (!string.IsNullOrEmpty(clientIp))
        {
            var now = DateTime.UtcNow;
            var key = clientIp;

            // Limpiar entradas antiguas periódicamente
            if (_requestCounts.Count > 10000)
            {
                CleanupOldEntries(now);
            }

            // Verificar rate limit
            var rateLimitInfo = _requestCounts.AddOrUpdate(
                key,
                new RateLimitInfo { Count = 1, ResetTime = now.AddSeconds(TimeWindowSeconds) },
                (k, existing) =>
                {
                    if (now > existing.ResetTime)
                    {
                        // Ventana de tiempo expirada, resetear
                        return new RateLimitInfo { Count = 1, ResetTime = now.AddSeconds(TimeWindowSeconds) };
                    }
                    
                    existing.Count++;
                    return existing;
                });

            // Verificar si excedió el límite
            if (rateLimitInfo.Count > MaxRequestsPerMinute)
            {
                _logger.LogWarning("Rate limit excedido para IP: {Ip}", clientIp);
                
                context.Response.StatusCode = 429; // Too Many Requests
                context.Response.Headers["Retry-After"] = TimeWindowSeconds.ToString();
                await context.Response.WriteAsync("Demasiadas peticiones. Intenta más tarde.");
                return;
            }
        }

        await _next(context);
    }

    /// <summary>
    /// Obtiene la IP del cliente sin exponerla en logs públicos
    /// </summary>
    private string GetClientIpAddress(HttpContext context)
    {
        // Intentar obtener IP desde headers (útil si hay proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // IP directa
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private void CleanupOldEntries(DateTime now)
    {
        var keysToRemove = _requestCounts
            .Where(kvp => now > kvp.Value.ResetTime)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _requestCounts.TryRemove(key, out _);
        }
    }

    private class RateLimitInfo
    {
        public int Count { get; set; }
        public DateTime ResetTime { get; set; }
    }
}

