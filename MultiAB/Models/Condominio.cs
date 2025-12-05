using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiAB.Models;

/// <summary>
/// Entidad de Condominio con soporte multi-tenant
/// </summary>
[Table("Condominios")]
public class Condominio : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Direccion { get; set; }

    [MaxLength(100)]
    public string? Ciudad { get; set; }

    [MaxLength(50)]
    public string? CodigoPostal { get; set; }

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(255)]
    [EmailAddress]
    public string? Email { get; set; }

    public int TotalCasas { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Multi-tenant
    [Required]
    [MaxLength(50)]
    public string TenantId { get; set; } = string.Empty;

    // Relaciones
    public virtual ICollection<Casa> Casas { get; set; } = new List<Casa>();
}






