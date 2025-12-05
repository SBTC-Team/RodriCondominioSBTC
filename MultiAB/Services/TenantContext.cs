using System.Threading;

namespace MultiAB.Services;

/// <summary>
/// Contexto estático para almacenar el tenant actual por request
/// Usa AsyncLocal para mantener el valor por hilo de ejecución asíncrono
/// </summary>
public static class TenantContext
{
    private static readonly AsyncLocal<string?> _currentTenant = new AsyncLocal<string?>();

    /// <summary>
    /// Campo estático para uso en expresiones EF Core HasQueryFilter
    /// EF Core puede leer este campo directamente en las expresiones lambda
    /// Usar campo en lugar de propiedad para mejor compatibilidad con EF Core
    /// </summary>
    public static string CurrentTenantId
    {
        get => _currentTenant.Value ?? string.Empty;
    }

    /// <summary>
    /// Obtiene el tenant actual del contexto
    /// </summary>
    public static string? CurrentTenant
    {
        get => _currentTenant.Value;
        set => _currentTenant.Value = value;
    }

    /// <summary>
    /// Establece el tenant actual
    /// </summary>
    public static void SetTenant(string tenantId)
    {
        CurrentTenant = tenantId;
    }

    /// <summary>
    /// Limpia el tenant actual
    /// </summary>
    public static void Clear()
    {
        CurrentTenant = null;
    }
}

