namespace MultiAB.Models;

/// <summary>
/// Entidad de registro de auditoría con soporte multi-tenant
/// Validaciones de dominio en setters para evitar estados inválidos
/// </summary>
public class AuditLog : AuditableEntity, ITenantEntity
{
    public int Id { get; internal set; }

    private string _entityName = string.Empty;
    
    /// <summary>
    /// Nombre de la entidad auditada
    /// </summary>
    public string EntityName
    {
        get => _entityName;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("El nombre de la entidad no puede estar vacío", nameof(EntityName));
            }
            if (value.Length > 100)
            {
                throw new ArgumentException("El nombre de la entidad no puede exceder 100 caracteres", nameof(EntityName));
            }
            _entityName = value.Trim();
        }
    }

    private string? _entityId;
    
    /// <summary>
    /// ID de la entidad auditada
    /// </summary>
    public string? EntityId
    {
        get => _entityId;
        set
        {
            if (value != null && value.Length > 50)
            {
                throw new ArgumentException("El EntityId no puede exceder 50 caracteres", nameof(EntityId));
            }
            _entityId = value?.Trim();
        }
    }

    private string _action = string.Empty;
    
    /// <summary>
    /// Acción realizada (Create, Update, Delete, etc.)
    /// </summary>
    public string Action
    {
        get => _action;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("La acción no puede estar vacía", nameof(Action));
            }
            if (value.Length > 50)
            {
                throw new ArgumentException("La acción no puede exceder 50 caracteres", nameof(Action));
            }
            _action = value.Trim();
        }
    }

    private string? _userId;
    
    /// <summary>
    /// ID del usuario que realizó la acción
    /// </summary>
    public string? UserId
    {
        get => _userId;
        set
        {
            if (value != null && value.Length > 100)
            {
                throw new ArgumentException("El UserId no puede exceder 100 caracteres", nameof(UserId));
            }
            _userId = value?.Trim();
        }
    }

    private string? _description;
    
    /// <summary>
    /// Descripción de la acción
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

    /// <summary>
    /// Cambios realizados (JSON)
    /// </summary>
    public string? Changes { get; set; }

    private string? _ipAddress;
    
    /// <summary>
    /// Dirección IP (información privada, no se expone en respuestas)
    /// </summary>
    public string? IpAddress
    {
        get => _ipAddress;
        set
        {
            if (value != null && value.Length > 45)
            {
                throw new ArgumentException("La dirección IP no puede exceder 45 caracteres", nameof(IpAddress));
            }
            _ipAddress = value?.Trim();
        }
    }

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

    /// <summary>
    /// Constructor privado para EF Core
    /// </summary>
    private AuditLog() { }

    /// <summary>
    /// Constructor con validaciones de dominio
    /// </summary>
    public AuditLog(string entityName, string action, string tenantId, string? createdBy = null)
    {
        // Validaciones de dominio en constructor
        if (string.IsNullOrWhiteSpace(entityName))
        {
            throw new ArgumentException("El nombre de la entidad es requerido", nameof(entityName));
        }
        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("La acción es requerida", nameof(action));
        }
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException("El TenantId es requerido", nameof(tenantId));
        }

        EntityName = entityName;
        Action = action;
        TenantId = tenantId;
        SetCreated(createdBy);
    }
}
