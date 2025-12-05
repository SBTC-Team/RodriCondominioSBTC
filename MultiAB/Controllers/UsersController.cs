using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiAB.Data;
using MultiAB.Models;
using MultiAB.Services;
using System.ComponentModel.DataAnnotations;

namespace MultiAB.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UsersController> _logger;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IInputSanitizer _inputSanitizer;
    private readonly ITenantProvider _tenantProvider;

    public UsersController(
        ApplicationDbContext context, 
        ILogger<UsersController> logger,
        IPasswordHasher passwordHasher,
        IInputSanitizer inputSanitizer,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _logger = logger;
        _passwordHasher = passwordHasher;
        _inputSanitizer = inputSanitizer;
        _tenantProvider = tenantProvider;
    }

    /// <summary>
    /// Obtiene todos los usuarios del tenant actual
    /// El filtro global multi-tenant se aplica automáticamente
    /// SEGURIDAD: No expone PasswordHash
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetUsers()
    {
        // Esta consulta automáticamente filtra por el TenantId actual
        // gracias al HasQueryFilter configurado en ApplicationDbContext
        var users = await _context.Users.ToListAsync();
        
        // SEGURIDAD: No exponer PasswordHash en la respuesta
        var safeUsers = users.Select(u => new
        {
            u.Id,
            u.Username,
            u.Email,
            u.FirstName,
            u.LastName,
            u.IsActive,
            u.CreatedAt,
            u.UpdatedAt
            // PasswordHash NO se incluye
        });
        
        return Ok(safeUsers);
    }

    /// <summary>
    /// Obtiene un usuario por ID (solo del tenant actual)
    /// SEGURIDAD: No expone información sensible como PasswordHash
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        
        if (user == null)
        {
            // SEGURIDAD: No exponer si el ID existe o no (evita enumeración)
            return NotFound(new { Message = "Recurso no encontrado" });
        }

        // SEGURIDAD: No devolver PasswordHash
        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.IsActive,
            user.CreatedAt,
            user.UpdatedAt
            // PasswordHash NO se incluye
        });
    }

    /// <summary>
    /// Crea un nuevo usuario (automáticamente asignado al tenant actual)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser([FromBody] CreateUserDto dto)
    {
        // Validación de modelo
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Message = "Datos inválidos" });
        }

        // SEGURIDAD: Sanitizar entrada
        dto.Username = _inputSanitizer.SanitizeString(dto.Username);
        dto.Email = _inputSanitizer.SanitizeString(dto.Email);
        dto.FirstName = _inputSanitizer.SanitizeString(dto.FirstName ?? string.Empty);
        dto.LastName = _inputSanitizer.SanitizeString(dto.LastName ?? string.Empty);

        // SEGURIDAD: Validar email
        if (!_inputSanitizer.IsValidEmail(dto.Email))
        {
            return BadRequest(new { Message = "Email inválido" });
        }

        // SEGURIDAD: Detectar inyección SQL
        if (_inputSanitizer.ContainsSqlInjection(dto.Username) || 
            _inputSanitizer.ContainsSqlInjection(dto.Email) ||
            _inputSanitizer.ContainsXss(dto.Username) ||
            _inputSanitizer.ContainsXss(dto.Email))
        {
            _logger.LogWarning("Intento de inyección detectado desde IP: {Ip}", 
                HttpContext.Connection.RemoteIpAddress?.ToString());
            return BadRequest(new { Message = "Datos inválidos" });
        }

        // Verificar si el usuario ya existe (por username o email)
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == dto.Username || u.Email == dto.Email);
        
        if (existingUser != null)
        {
            // SEGURIDAD: No exponer si es username o email el que existe (evita enumeración)
            return BadRequest(new { Message = "El usuario o email ya existe" });
        }

        // Hashear la contraseña antes de guardarla
        var hashedPassword = _passwordHasher.HashPassword(dto.Password);

        // Obtener TenantId actual (se asignará automáticamente, pero lo necesitamos para el constructor)
        var tenantId = _tenantProvider.GetTenantId() ?? "default-tenant";

        // Usar constructor con validaciones de dominio
        var user = new User(
            username: dto.Username,
            email: dto.Email,
            passwordHash: hashedPassword,
            tenantId: tenantId,
            createdBy: null // Se puede obtener del contexto de autenticación si está disponible
        );

        // Asignar propiedades opcionales
        if (!string.IsNullOrWhiteSpace(dto.FirstName))
        {
            user.FirstName = dto.FirstName;
        }
        if (!string.IsNullOrWhiteSpace(dto.LastName))
        {
            user.LastName = dto.LastName;
        }
        user.IsActive = true;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // No devolver la contraseña hasheada en la respuesta
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new
        {
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.IsActive,
            user.CreatedAt
        });
    }

    /// <summary>
    /// Actualiza un usuario existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        // Validación de modelo
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.Username = dto.Username;
        user.Email = dto.Email;
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.IsActive = dto.IsActive;
        user.SetUpdated();

        // Hashear la contraseña si se proporciona una nueva
        if (!string.IsNullOrEmpty(dto.Password))
        {
            user.PasswordHash = _passwordHasher.HashPassword(dto.Password); // ✅ Contraseña hasheada
        }

        await _context.SaveChangesAsync();

        // No devolver la contraseña hasheada en la respuesta
        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.IsActive,
            user.UpdatedAt
        });
    }

    /// <summary>
    /// Elimina un usuario
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class UpdateUserDto
{
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 100 caracteres")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El email no es válido")]
    [StringLength(255, ErrorMessage = "El email no puede exceder 255 caracteres")]
    public string Email { get; set; } = string.Empty;

    [StringLength(500, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string? Password { get; set; }

    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string? FirstName { get; set; }

    [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
    public string? LastName { get; set; }

    public bool IsActive { get; set; } = true;
}

public class CreateUserDto
{
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 100 caracteres")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El email no es válido")]
    [StringLength(255, ErrorMessage = "El email no puede exceder 255 caracteres")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    [StringLength(500, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string Password { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string? FirstName { get; set; }

    [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
    public string? LastName { get; set; }
}

