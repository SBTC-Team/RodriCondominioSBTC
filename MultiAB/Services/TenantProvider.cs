namespace MultiAB.Services;

/// <summary>
/// Implementación del proveedor de tenant
/// En una aplicación real, esto podría obtener el tenant desde:
/// - Header HTTP (X-Tenant-Id)
/// - Claims del usuario autenticado
/// - Subdominio
/// - Base de datos de configuración
/// </summary>
public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string DefaultTenantId = "default-tenant"; // Para desarrollo/testing

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetTenantId()
    {
        // Primero intentar obtener del contexto estático (ya establecido por middleware)
        if (!string.IsNullOrEmpty(TenantContext.CurrentTenant))
        {
            return TenantContext.CurrentTenant;
        }

        // Intentar obtener el tenant desde el header HTTP
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantId) == true)
        {
            var tenant = tenantId.ToString();
            TenantContext.SetTenant(tenant); // Almacenar en contexto para uso en queries
            return tenant;
        }

        // Intentar obtener desde claims (si hay autenticación)
        var tenantClaim = httpContext?.User?.FindFirst("TenantId")?.Value;
        if (!string.IsNullOrEmpty(tenantClaim))
        {
            TenantContext.SetTenant(tenantClaim); // Almacenar en contexto para uso en queries
            return tenantClaim;
        }

        // Valor por defecto para desarrollo
        TenantContext.SetTenant(DefaultTenantId);
        return DefaultTenantId;
    }
}

