using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiAB.Models;

/// <summary>
/// Entidad de Casa con soporte multi-tenant
/// </summary>
[Table("Casas")]
public class Casa : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string NumeroCasa { get; set; } = string.Empty; // Ej: "A-101", "B-205"

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    [MaxLength(50)]
    public string? Tipo { get; set; } // Ej: "Departamento", "Casa", "Local"

    public decimal? AreaMetros { get; set; }

    public int? NumeroHabitaciones { get; set; }

    public int? NumeroBanos { get; set; }

    public bool TieneGaraje { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsOcupada { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Relación con Condominio
    [Required]
    public int CondominioId { get; set; }

    // Multi-tenant
    [Required]
    [MaxLength(50)]
    public string TenantId { get; set; } = string.Empty;

    // Navegación
    [ForeignKey(nameof(CondominioId))]
    public virtual Condominio Condominio { get; set; } = null!;
}






