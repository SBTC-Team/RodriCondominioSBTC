using MultiAB.Services;

namespace MultiAB.Middleware;

/// <summary>
/// Middleware para establecer el tenant actual antes de procesar cada request
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
    {
        // Establecer el tenant actual antes de procesar el request
        // Esto asegura que el filtro global tenga acceso al tenant correcto
        // Obtenemos el servicio desde el HttpContext (scope disponible aquí)
        var tenantId = tenantProvider.GetTenantId();
        TenantContext.SetTenant(tenantId);

        await _next(context);

        // Limpiar el tenant después de procesar el request
        TenantContext.Clear();
    }
}

