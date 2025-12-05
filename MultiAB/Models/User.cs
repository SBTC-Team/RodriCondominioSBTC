namespace MultiAB.Models;

/// <summary>
/// Entidad de Usuario con soporte multi-tenant
/// Validaciones de dominio en setters para evitar estados inválidos
/// </summary>
public class User : AuditableEntity, ITenantEntity
{
    public int Id { get; internal set; }

    private string _username = string.Empty;
    
    /// <summary>
    /// Nombre de usuario
    /// Validación: No puede estar vacío, máximo 100 caracteres
    /// </summary>
    public string Username
    {
        get => _username;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("El nombre de usuario no puede estar vacío", nameof(Username));
            }
            if (value.Length > 100)
            {
                throw new ArgumentException("El nombre de usuario no puede exceder 100 caracteres", nameof(Username));
            }
            _username = value.Trim();
        }
    }

    private string _email = string.Empty;
    
    /// <summary>
    /// Email del usuario
    /// Validación: Debe ser un email válido, máximo 255 caracteres
    /// </summary>
    public string Email
    {
        get => _email;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("El email no puede estar vacío", nameof(Email));
            }
            if (value.Length > 255)
            {
                throw new ArgumentException("El email no puede exceder 255 caracteres", nameof(Email));
            }
            if (!value.Contains('@') || !value.Contains('.'))
            {
                throw new ArgumentException("El email debe tener un formato válido", nameof(Email));
            }
            _email = value.Trim().ToLowerInvariant();
        }
    }

    private string _passwordHash = string.Empty;
    
    /// <summary>
    /// Hash de la contraseña (nunca se expone en texto plano)
    /// </summary>
    public string PasswordHash
    {
        get => _passwordHash;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("El hash de contraseña no puede estar vacío", nameof(PasswordHash));
            }
            if (value.Length > 500)
            {
                throw new ArgumentException("El hash de contraseña no puede exceder 500 caracteres", nameof(PasswordHash));
            }
            _passwordHash = value;
        }
    }

    private string? _firstName;
    
    /// <summary>
    /// Nombre del usuario
    /// </summary>
    public string? FirstName
    {
        get => _firstName;
        set
        {
            if (value != null && value.Length > 100)
            {
                throw new ArgumentException("El nombre no puede exceder 100 caracteres", nameof(FirstName));
            }
            _firstName = value?.Trim();
        }
    }

    private string? _lastName;
    
    /// <summary>
    /// Apellido del usuario
    /// </summary>
    public string? LastName
    {
        get => _lastName;
        set
        {
            if (value != null && value.Length > 100)
            {
                throw new ArgumentException("El apellido no puede exceder 100 caracteres", nameof(LastName));
            }
            _lastName = value?.Trim();
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
    private User() { }

    /// <summary>
    /// Constructor con validaciones de dominio
    /// </summary>
    public User(string username, string email, string passwordHash, string tenantId, string? createdBy = null)
    {
        // Validaciones de dominio en constructor
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("El nombre de usuario es requerido", nameof(username));
        }
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("El email es requerido", nameof(email));
        }
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("El hash de contraseña es requerido", nameof(passwordHash));
        }
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException("El TenantId es requerido", nameof(tenantId));
        }

        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        TenantId = tenantId;
        SetCreated(createdBy);
    }
}
