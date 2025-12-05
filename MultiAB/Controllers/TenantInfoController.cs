using Microsoft.AspNetCore.Mvc;
using MultiAB.Services;
using Microsoft.AspNetCore.Hosting;

namespace MultiAB.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantInfoController : ControllerBase
{
    private readonly ITenantProvider _tenantProvider;

    public TenantInfoController(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    /// <summary>
    /// Obtiene información del tenant actual
    /// Útil para verificar qué tenant está activo
    /// SEGURIDAD: Solo disponible en Development
    /// </summary>
    [HttpGet]
    public IActionResult GetCurrentTenant()
    {
        // SEGURIDAD: Solo permitir en Development
        if (!HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            return NotFound(new { Message = "Recurso no encontrado" });
        }

        var tenantId = _tenantProvider.GetTenantId();
        return Ok(new
        {
            TenantId = tenantId,
            Message = $"El tenant actual es: {tenantId}",
            Instructions = new
            {
                Header = "Para cambiar el tenant, envía el header:",
                HeaderName = "X-Tenant-Id",
                Example = "X-Tenant-Id: tenant-1"
            }
        });
    }
}










