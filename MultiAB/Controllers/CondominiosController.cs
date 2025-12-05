using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiAB.Data;
using MultiAB.Models;

namespace MultiAB.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CondominiosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CondominiosController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene todos los condominios del tenant actual
    /// El filtro global multi-tenant se aplica automáticamente
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Condominio>>> GetCondominios()
    {
        var condominios = await _context.Condominios
            .Include(c => c.Casas)
            .ToListAsync();
        return Ok(condominios);
    }

    /// <summary>
    /// Obtiene un condominio por ID (solo del tenant actual)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Condominio>> GetCondominio(int id)
    {
        var condominio = await _context.Condominios
            .Include(c => c.Casas)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (condominio == null)
        {
            return NotFound();
        }

        return Ok(condominio);
    }

    /// <summary>
    /// Crea un nuevo condominio (automáticamente asignado al tenant actual)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Condominio>> CreateCondominio([FromBody] CreateCondominioDto dto)
    {
        var condominio = new Condominio
        {
            Nombre = dto.Nombre,
            Direccion = dto.Direccion,
            Ciudad = dto.Ciudad,
            CodigoPostal = dto.CodigoPostal,
            Telefono = dto.Telefono,
            Email = dto.Email,
            TotalCasas = dto.TotalCasas,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
            // TenantId se asigna automáticamente
        };

        _context.Condominios.Add(condominio);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCondominio), new { id = condominio.Id }, condominio);
    }

    /// <summary>
    /// Actualiza un condominio existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCondominio(int id, [FromBody] UpdateCondominioDto dto)
    {
        var condominio = await _context.Condominios.FindAsync(id);
        if (condominio == null)
        {
            return NotFound();
        }

        condominio.Nombre = dto.Nombre;
        condominio.Direccion = dto.Direccion;
        condominio.Ciudad = dto.Ciudad;
        condominio.CodigoPostal = dto.CodigoPostal;
        condominio.Telefono = dto.Telefono;
        condominio.Email = dto.Email;
        condominio.TotalCasas = dto.TotalCasas;
        condominio.IsActive = dto.IsActive;
        condominio.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(condominio);
    }

    /// <summary>
    /// Elimina un condominio
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCondominio(int id)
    {
        var condominio = await _context.Condominios.FindAsync(id);
        if (condominio == null)
        {
            return NotFound();
        }

        _context.Condominios.Remove(condominio);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class UpdateCondominioDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public int TotalCasas { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateCondominioDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public int TotalCasas { get; set; }
}


