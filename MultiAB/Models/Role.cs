namespace MultiAB.Models;

/// <summary>
/// Entidad de Rol con soporte multi-tenant
/// Validaciones de dominio en setters para evitar estados inválidos
/// </summary>
public class Role : AuditableEntity, ITenantEntity
{
    public int Id { get; internal set; }

    private string _name = string.Empty;
    
    /// <summary>
    /// Nombre del rol
    /// Validación: No puede estar vacío, máximo 100 caracteres
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("El nombre del rol no puede estar vacío", nameof(Name));
            }
            if (value.Length > 100)
            {
                throw new ArgumentException("El nombre del rol no puede exceder 100 caracteres", nameof(Name));
            }
            _name = value.Trim();
        }
    }

    private string? _description;
    
    /// <summary>
    /// Descripción del rol
    /// </summary>
    public string? Description
    {
        get => _description;
        set
        {
            if (value != null && value.Length > 500)
            {
                throw new ArgumentException("La descripción no puede exceder 500 caracteres", nameof(Description));
            }
            _description = value?.Trim();
        }
    }

    public bool IsActive { get; set; } = true;

    private string _tenantId = string.Empty;
    
    /// <summary>
    /// Identificador del tenant (cliente)
    /// </summary>
    public string TenantId
    {
        get => _tenantId;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("El TenantId no puede estar vacío", nameof(TenantId));
            }
            if (value.Length > 50)
            {
                throw new ArgumentException("El TenantId no puede exceder 50 caracteres", nameof(TenantId));
            }
            _tenantId = value.Trim();
        }
    }

    // Relaciones
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    /// <summary>
    /// Constructor privado para EF Core
    /// </summary>
    private Role() { }

    /// <summary>
    /// Constructor con validaciones de dominio
    /// </summary>
    public Role(string name, string tenantId, string? createdBy = null)
    {
        // Validaciones de dominio en constructor
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("El nombre del rol es requerido", nameof(name));
        }
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException("El TenantId es requerido", nameof(tenantId));
        }

        Name = name;
        TenantId = tenantId;
        SetCreated(createdBy);
    }
}
