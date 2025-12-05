using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiAB.Data;
using MultiAB.Models;
using MultiAB.Services;

namespace MultiAB.Controllers;

/// <summary>
/// Controlador para gestionar Tenants (Clientes)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public TenantsController(ApplicationDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    /// <summary>
    /// Obtiene todos los tenants del sistema
    /// El filtro global multi-tenant se aplica automáticamente
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetTenants()
    {
        var tenants = await _context.Tenants.ToListAsync();
        
        // No exponer información sensible
        var safeTenants = tenants.Select(t => new
        {
            t.Id,
            t.Name,
            t.Description,
            t.TenantId,
            t.IsActive,
            t.CreatedAt,
            t.UpdatedAt
        });
        
        return Ok(safeTenants);
    }

    /// <summary>
    /// Obtiene un tenant por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetTenant(int id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        
        if (tenant == null)
        {
            return NotFound(new { Message = "Recurso no encontrado" });
        }

        return Ok(new
        {
            tenant.Id,
            tenant.Name,
            tenant.Description,
            tenant.TenantId,
            tenant.IsActive,
            tenant.CreatedAt,
            tenant.UpdatedAt
        });
    }

    /// <summary>
    /// Crea un nuevo tenant
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<object>> CreateTenant([FromBody] CreateTenantDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Message = "Datos inválidos" });
        }

        // Validar que el TenantId sea único
        var existingTenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.TenantId == dto.TenantId);
        
        if (existingTenant != null)
        {
            return BadRequest(new { Message = "El TenantId ya existe" });
        }

        // Usar constructor con validaciones de dominio
        var tenant = new Tenant(
            name: dto.Name,
            tenantId: dto.TenantId,
            createdBy: null // Se puede obtener del contexto de autenticación si está disponible
        );

        if (!string.IsNullOrWhiteSpace(dto.Description))
        {
            tenant.Description = dto.Description;
        }

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTenant), new { id = tenant.Id }, new
        {
            tenant.Id,
            tenant.Name,
            tenant.Description,
            tenant.TenantId,
            tenant.IsActive,
            tenant.CreatedAt
        });
    }

    /// <summary>
    /// Actualiza un tenant existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTenant(int id, [FromBody] UpdateTenantDto dto)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null)
        {
            return NotFound(new { Message = "Recurso no encontrado" });
        }

        // Usar setters con validaciones de dominio
        tenant.Name = dto.Name;
        if (!string.IsNullOrWhiteSpace(dto.Description))
        {
            tenant.Description = dto.Description;
        }
        tenant.IsActive = dto.IsActive;
        tenant.SetUpdated();

        await _context.SaveChangesAsync();

        return Ok(new
        {
            tenant.Id,
            tenant.Name,
            tenant.Description,
            tenant.TenantId,
            tenant.IsActive,
            tenant.UpdatedAt
        });
    }

    /// <summary>
    /// Elimina un tenant
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTenant(int id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null)
        {
            return NotFound(new { Message = "Recurso no encontrado" });
        }

        _context.Tenants.Remove(tenant);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateTenantDto
{
    public string Name { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateTenantDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

