using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiAB.Models;

/// <summary>
/// Tabla de relación muchos a muchos entre User y Role
/// </summary>
[Table("UserRoles")]
public class UserRole : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int RoleId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Multi-tenant
    [Required]
    [MaxLength(50)]
    public string TenantId { get; set; } = string.Empty;

    // Navegación
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [ForeignKey(nameof(RoleId))]
    public virtual Role Role { get; set; } = null!;
}










