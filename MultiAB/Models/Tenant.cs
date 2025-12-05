namespace MultiAB.Models;

/// <summary>
/// Entidad Tenant (Cliente) - Representa un cliente en el sistema multi-tenant
/// Validaciones de dominio en constructor para evitar estados inválidos
/// </summary>
public class Tenant : AuditableEntity, ITenantEntity
{
    public int Id { get; internal set; }

    private string _name = string.Empty;
    
    /// <summary>
    /// Nombre del tenant (cliente)
    /// Validación: No puede estar vacío
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("El nombre del tenant no puede estar vacío", nameof(Name));
            }
            if (value.Length > 200)
            {
                throw new ArgumentException("El nombre del tenant no puede exceder 200 caracteres", nameof(Name));
            }
            _name = value.Trim();
        }
    }

    private string? _description;
    
    /// <summary>
    /// Descripción del tenant
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

    private string _tenantId = string.Empty;
    
    /// <summary>
    /// Identificador único del tenant (usado para multi-tenancy)
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

    public bool IsActive { get; set; } = true;

    // Relaciones
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    /// <summary>
    /// Constructor privado para EF Core
    /// </summary>
    private Tenant() { }

    /// <summary>
    /// Constructor con validaciones de dominio
    /// </summary>
    public Tenant(string name, string tenantId, string? createdBy = null)
    {
        // Validaciones de dominio en constructor
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("El nombre del tenant es requerido", nameof(name));
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

