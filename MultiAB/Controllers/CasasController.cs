using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiAB.Data;
using MultiAB.Models;

namespace MultiAB.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CasasController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CasasController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene todas las casas del tenant actual
    /// El filtro global multi-tenant se aplica automáticamente
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Casa>>> GetCasas([FromQuery] int? condominioId = null)
    {
        var query = _context.Casas.Include(c => c.Condominio).AsQueryable();

        if (condominioId.HasValue)
        {
            query = query.Where(c => c.CondominioId == condominioId.Value);
        }

        var casas = await query.ToListAsync();
        return Ok(casas);
    }

    /// <summary>
    /// Obtiene una casa por ID (solo del tenant actual)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Casa>> GetCasa(int id)
    {
        var casa = await _context.Casas
            .Include(c => c.Condominio)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (casa == null)
        {
            return NotFound();
        }

        return Ok(casa);
    }

    /// <summary>
    /// Crea una nueva casa (automáticamente asignada al tenant actual)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Casa>> CreateCasa([FromBody] CreateCasaDto dto)
    {
        // Verificar que el condominio existe y pertenece al mismo tenant
        var condominio = await _context.Condominios.FindAsync(dto.CondominioId);
        if (condominio == null)
        {
            return BadRequest("El condominio especificado no existe o no pertenece al tenant actual");
        }

        var casa = new Casa
        {
            NumeroCasa = dto.NumeroCasa,
            Descripcion = dto.Descripcion,
            Tipo = dto.Tipo,
            AreaMetros = dto.AreaMetros,
            NumeroHabitaciones = dto.NumeroHabitaciones,
            NumeroBanos = dto.NumeroBanos,
            TieneGaraje = dto.TieneGaraje,
            IsActive = true,
            IsOcupada = false,
            CondominioId = dto.CondominioId,
            CreatedAt = DateTime.UtcNow
            // TenantId se asigna automáticamente
        };

        _context.Casas.Add(casa);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCasa), new { id = casa.Id }, casa);
    }

    /// <summary>
    /// Actualiza una casa existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCasa(int id, [FromBody] UpdateCasaDto dto)
    {
        var casa = await _context.Casas.FindAsync(id);
        if (casa == null)
        {
            return NotFound();
        }

        // Verificar que el condominio existe si se cambió
        if (dto.CondominioId != casa.CondominioId)
        {
            var condominio = await _context.Condominios.FindAsync(dto.CondominioId);
            if (condominio == null)
            {
                return BadRequest("El condominio especificado no existe o no pertenece al tenant actual");
            }
            casa.CondominioId = dto.CondominioId;
        }

        casa.NumeroCasa = dto.NumeroCasa;
        casa.Descripcion = dto.Descripcion;
        casa.Tipo = dto.Tipo;
        casa.AreaMetros = dto.AreaMetros;
        casa.NumeroHabitaciones = dto.NumeroHabitaciones;
        casa.NumeroBanos = dto.NumeroBanos;
        casa.TieneGaraje = dto.TieneGaraje;
        casa.IsActive = dto.IsActive;
        casa.IsOcupada = dto.IsOcupada;
        casa.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(casa);
    }

    /// <summary>
    /// Elimina una casa
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCasa(int id)
    {
        var casa = await _context.Casas.FindAsync(id);
        if (casa == null)
        {
            return NotFound();
        }

        _context.Casas.Remove(casa);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class UpdateCasaDto
{
    public string NumeroCasa { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Tipo { get; set; }
    public decimal? AreaMetros { get; set; }
    public int? NumeroHabitaciones { get; set; }
    public int? NumeroBanos { get; set; }
    public bool TieneGaraje { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsOcupada { get; set; } = false;
    public int CondominioId { get; set; }
}

public class CreateCasaDto
{
    public string NumeroCasa { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Tipo { get; set; }
    public decimal? AreaMetros { get; set; }
    public int? NumeroHabitaciones { get; set; }
    public int? NumeroBanos { get; set; }
    public bool TieneGaraje { get; set; }
    public int CondominioId { get; set; }
}


