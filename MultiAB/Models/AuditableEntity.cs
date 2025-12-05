namespace MultiAB.Models;

/// <summary>
/// Clase base para entidades que requieren auditoría
/// Implementa CreatedAt y CreatedBy según los requisitos de dominio
/// </summary>
public abstract class AuditableEntity
{
    /// <summary>
    /// Fecha de creación de la entidad
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// Usuario que creó la entidad
    /// </summary>
    public string? CreatedBy { get; protected set; }

    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Usuario que actualizó la entidad
    /// </summary>
    public string? UpdatedBy { get; protected set; }

    /// <summary>
    /// Establece la fecha de creación y el usuario que creó
    /// </summary>
    public void SetCreated(string? createdBy = null)
    {
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
    }

    /// <summary>
    /// Establece la fecha de actualización y el usuario que actualizó
    /// </summary>
    public void SetUpdated(string? updatedBy = null)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
}

