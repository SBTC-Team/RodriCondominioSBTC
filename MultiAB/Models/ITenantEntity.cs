namespace MultiAB.Models;

/// <summary>
/// Interfaz para entidades que requieren aislamiento multi-tenant
/// </summary>
public interface ITenantEntity
{
    /// <summary>
    /// Identificador del tenant (cliente)
    /// </summary>
    string TenantId { get; set; }
}










