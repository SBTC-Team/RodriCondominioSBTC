using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiAB.Data;
using MultiAB.Models;
using MultiAB.Services;

namespace MultiAB.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public AuditLogsController(ApplicationDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    /// <summary>
    /// Obtiene todos los logs de auditoría del tenant actual
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditLog>>> GetAuditLogs(
        [FromQuery] string? entityName = null,
        [FromQuery] string? entityId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(entityName))
        {
            query = query.Where(a => a.EntityName == entityName);
        }

        if (!string.IsNullOrEmpty(entityId))
        {
            query = query.Where(a => a.EntityId == entityId);
        }

        var totalCount = await query.CountAsync();
        var auditLogs = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // SEGURIDAD: No exponer IpAddress (información privada)
        var safeAuditLogs = auditLogs.Select(a => new
        {
            a.Id,
            a.EntityName,
            a.EntityId,
            a.Action,
            a.Description,
            a.Changes,
            a.UserId,
            // SEGURIDAD: No exponer IpAddress
            a.CreatedAt
        });

        return Ok(new
        {
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Data = safeAuditLogs
        });
    }

    /// <summary>
    /// Obtiene un log de auditoría por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AuditLog>> GetAuditLog(int id)
    {
        var auditLog = await _context.AuditLogs.FindAsync(id);

        if (auditLog == null)
        {
            return NotFound(new { Message = "Recurso no encontrado" });
        }

        // SEGURIDAD: No exponer IpAddress (información privada)
        return Ok(new
        {
            auditLog.Id,
            auditLog.EntityName,
            auditLog.EntityId,
            auditLog.Action,
            auditLog.Description,
            auditLog.Changes,
            auditLog.UserId,
            // SEGURIDAD: No exponer IpAddress
            auditLog.CreatedAt
        });
    }

    /// <summary>
    /// Crea un nuevo log de auditoría (automáticamente asignado al tenant actual)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AuditLog>> CreateAuditLog([FromBody] CreateAuditLogDto dto)
    {
        // Obtener TenantId actual
        var tenantId = _tenantProvider.GetTenantId() ?? "default-tenant";

        // Usar constructor con validaciones de dominio
        var auditLog = new AuditLog(
            entityName: dto.EntityName,
            action: dto.Action,
            tenantId: tenantId,
            createdBy: dto.UserId?.ToString()
        );

        // Asignar propiedades opcionales
        if (dto.EntityId != null)
        {
            auditLog.EntityId = dto.EntityId.ToString();
        }
        if (!string.IsNullOrWhiteSpace(dto.Changes))
        {
            auditLog.Changes = dto.Changes;
        }
        if (!string.IsNullOrWhiteSpace(dto.UserId?.ToString()))
        {
            auditLog.UserId = dto.UserId.ToString();
        }
        if (!string.IsNullOrWhiteSpace(dto.IpAddress))
        {
            auditLog.IpAddress = dto.IpAddress;
        }
        if (!string.IsNullOrWhiteSpace(dto.Description))
        {
            auditLog.Description = dto.Description;
        }

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        // SEGURIDAD: No exponer IpAddress en la respuesta
        var safeAuditLog = new
        {
            auditLog.Id,
            auditLog.EntityName,
            auditLog.EntityId,
            auditLog.Action,
            auditLog.Description,
            auditLog.Changes,
            auditLog.UserId,
            // SEGURIDAD: No exponer IpAddress
            auditLog.CreatedAt
        };

        return CreatedAtAction(nameof(GetAuditLog), new { id = auditLog.Id }, safeAuditLog);
    }
}

public class CreateAuditLogDto
{
    public string EntityName { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Changes { get; set; }
    public int? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? Description { get; set; }
}

