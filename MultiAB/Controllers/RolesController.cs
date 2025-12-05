using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiAB.Data;
using MultiAB.Models;
using MultiAB.Services;

namespace MultiAB.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public RolesController(ApplicationDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    /// <summary>
    /// Obtiene todos los roles del tenant actual
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
    {
        var roles = await _context.Roles.ToListAsync();
        return Ok(roles);
    }

    /// <summary>
    /// Crea un nuevo rol (automáticamente asignado al tenant actual)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Role>> CreateRole([FromBody] CreateRoleDto dto)
    {
        // Validación de modelo
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Message = "Datos inválidos" });
        }

        // Obtener TenantId actual
        var tenantId = _tenantProvider.GetTenantId() ?? "default-tenant";

        // Usar constructor con validaciones de dominio
        var role = new Role(
            name: dto.Name?.Trim() ?? string.Empty,
            tenantId: tenantId,
            createdBy: null // Se puede obtener del contexto de autenticación si está disponible
        );

        // Asignar propiedades opcionales
        if (!string.IsNullOrWhiteSpace(dto.Description))
        {
            role.Description = dto.Description?.Trim();
        }
        role.IsActive = true;

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRoles), new { id = role.Id }, role);
    }

    /// <summary>
    /// Obtiene un rol por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Role>> GetRole(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        
        if (role == null)
        {
            // SEGURIDAD: No exponer si el ID existe o no
            return NotFound(new { Message = "Recurso no encontrado" });
        }

        return Ok(role);
    }

    /// <summary>
    /// Actualiza un rol existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto dto)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        role.Name = dto.Name;
        role.Description = dto.Description;
        role.IsActive = dto.IsActive;
        role.SetUpdated();

        await _context.SaveChangesAsync();

        return Ok(role);
    }

    /// <summary>
    /// Elimina un rol
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class UpdateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}






