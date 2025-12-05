using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiAB.Data;
using MultiAB.Models;
using MultiAB.Services;

namespace MultiAB.Controllers;

/// <summary>
/// Controlador de demostración para ver EF Core en acción
/// Este controlador muestra cómo EF Core ejecuta consultas SQL
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EfCoreDemoController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EfCoreDemoController> _logger;

    public EfCoreDemoController(ApplicationDbContext context, ILogger<EfCoreDemoController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Muestra información sobre EF Core y ejecuta una consulta simple
    /// Abre la consola donde corre la aplicación para ver los logs de EF Core
    /// </summary>
    [HttpGet("info")]
    public IActionResult GetEfCoreInfo()
    {
        // SEGURIDAD: No exponer información del sistema (DatabaseProvider, etc.)
        return Ok(new
        {
            Message = "API funcionando correctamente",
            Status = "OK"
        });
    }

    /// <summary>
    /// Ejecuta una consulta simple con EF Core
    /// MIRA LA CONSOLA para ver el SQL que genera EF Core
    /// </summary>
    [HttpGet("query")]
    public async Task<IActionResult> ExecuteQuery()
    {
        _logger.LogInformation("=== INICIANDO CONSULTA EF CORE ===");
        
        // Esta línea hace que EF Core ejecute SQL
        // MIRA LA CONSOLA para ver el SQL generado
        var userCount = await _context.Users.CountAsync();
        
        _logger.LogInformation("=== CONSULTA COMPLETADA ===");
        
        return Ok(new
        {
            Message = "Consulta ejecutada por EF Core",
            UserCount = userCount,
            SqlGenerated = "Mira la consola - verás 'Executed DbCommand' con el SQL",
            Note = "EF Core generó automáticamente: SELECT COUNT(*) FROM Users WHERE TenantId = @tenantId"
        });
    }

    /// <summary>
    /// Ejecuta una consulta más compleja para ver EF Core trabajando
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsersWithEfCore()
    {
        _logger.LogInformation("=== EF CORE: Consultando usuarios ===");
        
        // EF Core ejecutará: SELECT * FROM Users WHERE TenantId = @tenantId
        var users = await _context.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.Username)
            .ToListAsync();
        
        _logger.LogInformation($"=== EF CORE: Encontrados {users.Count} usuarios ===");
        
        return Ok(new
        {
            Message = "Consulta ejecutada por EF Core",
            UsersFound = users.Count,
            Users = users.Select(u => new
            {
                u.Id,
                u.Username,
                // SEGURIDAD: No exponer Email ni TenantId
                u.IsActive
            }),
            Note = "Mira la consola para ver el SQL generado por EF Core"
        });
    }

    /// <summary>
    /// Muestra el SQL que EF Core generaría (sin ejecutarlo)
    /// </summary>
    [HttpGet("sql")]
    public async Task<IActionResult> GetGeneratedSql()
    {
        // Obtener el SQL que EF Core generaría
        var query = _context.Users.Where(u => u.IsActive);
        var sql = query.ToQueryString();
        
        return Ok(new
        {
            Message = "SQL que EF Core generaría para esta consulta",
            Sql = sql,
            Note = "Este es el SQL que EF Core traduce desde tu código C#"
        });
    }

    /// <summary>
    /// Crea un usuario usando EF Core (verás INSERT en los logs)
    /// </summary>
    [HttpPost("create-test-user")]
    public async Task<IActionResult> CreateTestUser()
    {
        _logger.LogInformation("=== EF CORE: Creando usuario ===");
        
        // SEGURIDAD: Usar PasswordHasher para hashear contraseñas
        var passwordHasher = HttpContext.RequestServices.GetRequiredService<IPasswordHasher>();
        
        // Obtener TenantId actual
        var tenantProvider = HttpContext.RequestServices.GetRequiredService<ITenantProvider>();
        var tenantId = tenantProvider.GetTenantId() ?? "default-tenant";
        
        // Usar constructor con validaciones de dominio
        var testUser = new User(
            username: $"test_user_{DateTime.Now:yyyyMMddHHmmss}",
            email: $"test_{DateTime.Now:yyyyMMddHHmmss}@test.com",
            passwordHash: passwordHasher.HashPassword("test123"), // SEGURIDAD: Hashear contraseña
            tenantId: tenantId
        );
        testUser.FirstName = "Test";
        testUser.LastName = "User";
        testUser.IsActive = true;
        testUser.SetCreated();

        _context.Users.Add(testUser);
        
        // MIRA LA CONSOLA - verás el INSERT que ejecuta EF Core
        await _context.SaveChangesAsync();
        
        _logger.LogInformation($"=== EF CORE: Usuario creado con ID {testUser.Id} ===");
        
        return Ok(new
        {
            Message = "Usuario creado usando EF Core",
            User = new
            {
                testUser.Id,
                testUser.Username,
                // SEGURIDAD: No exponer Email ni TenantId
                testUser.CreatedAt
            },
            Note = "Mira la consola - verás 'Executed DbCommand' con el INSERT SQL"
        });
    }

    /// <summary>
    /// Consulta Condominios (verás SELECT con filtro multi-tenant)
    /// </summary>
    [HttpGet("condominios")]
    public async Task<IActionResult> GetCondominiosWithEfCore()
    {
        _logger.LogInformation("=== EF CORE: Consultando condominios ===");
        
        // EF Core ejecutará: SELECT * FROM Condominios WHERE TenantId = @tenantId
        var condominios = await _context.Condominios
            .Include(c => c.Casas)
            .ToListAsync();
        
        _logger.LogInformation($"=== EF CORE: Encontrados {condominios.Count} condominios ===");
        
        return Ok(new
        {
            Message = "Consulta ejecutada por EF Core",
            CondominiosFound = condominios.Count,
            Condominios = condominios.Select(c => new
            {
                c.Id,
                c.Nombre,
                c.Ciudad,
                // SEGURIDAD: No exponer TenantId
                TotalCasas = c.Casas.Count,
                c.IsActive
            }),
            Note = "Mira la consola para ver el SQL generado por EF Core"
        });
    }

    /// <summary>
    /// Consulta Casas (verás SELECT con filtro multi-tenant)
    /// </summary>
    [HttpGet("casas")]
    public async Task<IActionResult> GetCasasWithEfCore()
    {
        _logger.LogInformation("=== EF CORE: Consultando casas ===");
        
        // EF Core ejecutará: SELECT * FROM Casas WHERE TenantId = @tenantId
        var casas = await _context.Casas
            .Include(c => c.Condominio)
            .ToListAsync();
        
        _logger.LogInformation($"=== EF CORE: Encontradas {casas.Count} casas ===");
        
        return Ok(new
        {
            Message = "Consulta ejecutada por EF Core",
            CasasFound = casas.Count,
            Casas = casas.Select(c => new
            {
                c.Id,
                c.NumeroCasa,
                c.Tipo,
                Condominio = c.Condominio.Nombre,
                // SEGURIDAD: No exponer TenantId
                c.IsOcupada,
                c.IsActive
            }),
            Note = "Mira la consola para ver el SQL generado por EF Core"
        });
    }

    /// <summary>
    /// Muestra los datos semilla (Seed Data): Tenant Demo, Admin User, Roles
    /// Este endpoint consulta los datos que se insertaron automáticamente al ejecutar las migraciones
    /// Usa IgnoreQueryFilters() para ver los datos semilla independientemente del tenant actual
    /// </summary>
    [HttpGet("seed-data")]
    public async Task<IActionResult> GetSeedData()
    {
        _logger.LogInformation("=== EF CORE: Consultando datos semilla ===");
        
        // Consultar datos semilla del tenant demo
        // IgnoreQueryFilters() permite ver los datos semilla sin el filtro multi-tenant
        // EF Core ejecutará: SELECT * FROM Tenants WHERE TenantId = 'demo-tenant'
        var tenants = await _context.Tenants
            .IgnoreQueryFilters()
            .Where(t => t.TenantId == "demo-tenant")
            .ToListAsync();
        
        // EF Core ejecutará: SELECT * FROM Roles WHERE TenantId = 'demo-tenant'
        var roles = await _context.Roles
            .IgnoreQueryFilters()
            .Where(r => r.TenantId == "demo-tenant")
            .ToListAsync();
        
        // EF Core ejecutará: SELECT * FROM Users WHERE TenantId = 'demo-tenant'
        var users = await _context.Users
            .IgnoreQueryFilters()
            .Where(u => u.TenantId == "demo-tenant")
            .ToListAsync();
        
        // EF Core ejecutará: SELECT * FROM UserRoles WHERE TenantId = 'demo-tenant'
        var userRoles = await _context.UserRoles
            .IgnoreQueryFilters()
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .Where(ur => ur.TenantId == "demo-tenant")
            .ToListAsync();
        
        _logger.LogInformation($"=== EF CORE: Datos semilla encontrados ===");
        _logger.LogInformation($"  - Tenants: {tenants.Count}");
        _logger.LogInformation($"  - Roles: {roles.Count}");
        _logger.LogInformation($"  - Users: {users.Count}");
        _logger.LogInformation($"  - UserRoles: {userRoles.Count}");
        
        return Ok(new
        {
            Message = "Datos semilla (Seed Data) consultados con EF Core",
            TenantDemo = "demo-tenant",
            SeedData = new
            {
                Tenants = tenants.Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.Description,
                    t.TenantId,
                    t.IsActive,
                    t.CreatedAt
                }),
                Roles = roles.Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Description,
                    r.IsActive,
                    // SEGURIDAD: No exponer TenantId
                    r.CreatedAt
                }),
                AdminUser = users.Select(u => new
                {
                    u.Id,
                    u.Username,
                    // SEGURIDAD: No exponer Email, FirstName, LastName ni TenantId
                    u.IsActive,
                    u.CreatedAt
                }),
                UserRoles = userRoles.Select(ur => new
                {
                    ur.Id,
                    User = ur.User.Username,
                    Role = ur.Role.Name,
                    ur.AssignedAt
                    // SEGURIDAD: No exponer TenantId
                })
            },
            Note = "Mira la consola para ver los logs de EF Core con las consultas SQL",
            Instructions = new
            {
                Step1 = "Estos datos se insertaron automáticamente al ejecutar: dotnet ef database update",
                Step2 = "Los datos semilla están en: Migrations/20251205165914_SeedData.cs",
                Step3 = "Mira la consola para ver el SQL que EF Core ejecutó"
            }
        });
    }

    /// <summary>
    /// Consulta Roles con EF Core (incluye datos semilla)
    /// </summary>
    [HttpGet("roles")]
    public async Task<IActionResult> GetRolesWithEfCore()
    {
        _logger.LogInformation("=== EF CORE: Consultando roles ===");
        
        // EF Core ejecutará: SELECT * FROM Roles WHERE TenantId = @tenantId
        var roles = await _context.Roles
            .OrderBy(r => r.Name)
            .ToListAsync();
        
        _logger.LogInformation($"=== EF CORE: Encontrados {roles.Count} roles ===");
        
        return Ok(new
        {
            Message = "Consulta ejecutada por EF Core",
            RolesFound = roles.Count,
            Roles = roles.Select(r => new
            {
                r.Id,
                r.Name,
                r.Description,
                r.IsActive,
                // SEGURIDAD: No exponer TenantId
                r.CreatedAt
            }),
            Note = "Mira la consola para ver el SQL generado por EF Core",
            SeedDataInfo = "Los roles Admin, User y Manager son datos semilla del tenant demo"
        });
    }
}



