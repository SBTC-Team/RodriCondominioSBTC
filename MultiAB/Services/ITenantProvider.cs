namespace MultiAB.Services;

/// <summary>
/// Interfaz para proveer el identificador del tenant actual
/// </summary>
public interface ITenantProvider
{
    /// <summary>
    /// Obtiene el identificador del tenant actual
    /// </summary>
    string GetTenantId();
}










