using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiAB.Data;
using MultiAB.Models;

namespace MultiAB.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserRolesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserRolesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene todas las asignaciones de roles a usuarios del tenant actual
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetUserRoles()
    {
        var userRoles = await _context.UserRoles
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .ToListAsync();

        var result = userRoles.Select(ur => new
        {
            ur.Id,
            UserId = ur.UserId,
            UserName = ur.User.Username,
            RoleId = ur.RoleId,
            RoleName = ur.Role.Name,
            ur.AssignedAt,
            ur.TenantId
        });

        return Ok(result);
    }

    /// <summary>
    /// Obtiene un UserRole por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetUserRole(int id)
    {
        var userRole = await _context.UserRoles
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .FirstOrDefaultAsync(ur => ur.Id == id);

        if (userRole == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            userRole.Id,
            UserId = userRole.UserId,
            UserName = userRole.User.Username,
            RoleId = userRole.RoleId,
            RoleName = userRole.Role.Name,
            userRole.AssignedAt,
            userRole.TenantId
        });
    }

    /// <summary>
    /// Asigna un rol a un usuario (automáticamente asignado al tenant actual)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserRole>> AssignRoleToUser([FromBody] CreateUserRoleDto dto)
    {
        // Verificar que el usuario existe y pertenece al tenant actual
        var user = await _context.Users.FindAsync(dto.UserId);
        if (user == null)
        {
            return BadRequest("El usuario especificado no existe o no pertenece al tenant actual");
        }

        // Verificar que el rol existe y pertenece al tenant actual
        var role = await _context.Roles.FindAsync(dto.RoleId);
        if (role == null)
        {
            return BadRequest("El rol especificado no existe o no pertenece al tenant actual");
        }

        // Verificar que no exista ya esta asignación
        var existing = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == dto.UserId && ur.RoleId == dto.RoleId);

        if (existing != null)
        {
            return BadRequest("Este usuario ya tiene asignado este rol");
        }

        var userRole = new UserRole
        {
            UserId = dto.UserId,
            RoleId = dto.RoleId,
            AssignedAt = DateTime.UtcNow
            // TenantId se asigna automáticamente
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUserRole), new { id = userRole.Id }, userRole);
    }

    /// <summary>
    /// Elimina una asignación de rol a usuario
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveUserRole(int id)
    {
        var userRole = await _context.UserRoles.FindAsync(id);
        if (userRole == null)
        {
            return NotFound();
        }

        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Obtiene todos los roles de un usuario específico
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetUserRolesByUser(int userId)
    {
        var userRoles = await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId)
            .ToListAsync();

        var result = userRoles.Select(ur => new
        {
            ur.Id,
            RoleId = ur.RoleId,
            RoleName = ur.Role.Name,
            RoleDescription = ur.Role.Description,
            ur.AssignedAt
        });

        return Ok(result);
    }
}

public class CreateUserRoleDto
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
}





