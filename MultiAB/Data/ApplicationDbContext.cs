using Microsoft.EntityFrameworkCore;
using MultiAB.Models;
using MultiAB.Services;
using MultiAB.Data.Configurations;

namespace MultiAB.Data;

/// <summary>
/// DbContext principal con soporte multi-tenant
/// </summary>
public class ApplicationDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
        // Establecer el tenant en el contexto estático al crear el DbContext
        // Esto asegura que el filtro global tenga acceso al tenant actual
        var tenantId = _tenantProvider.GetTenantId();
        TenantContext.SetTenant(tenantId);
    }

    // DbSets
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Condominio> Condominios { get; set; }
    public DbSet<Casa> Casas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configuraciones usando IEntityTypeConfiguration
        // Esto permite separar la configuración de cada entidad en su propia clase
        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
        modelBuilder.ApplyConfiguration(new CondominioConfiguration());
        modelBuilder.ApplyConfiguration(new CasaConfiguration());

        // Datos semilla (Seed Data)
        SeedData(modelBuilder);
    }

    /// <summary>
    /// Configura datos semilla para la base de datos
    /// </summary>
    private void SeedData(ModelBuilder modelBuilder)
    {
        const string demoTenantId = "demo-tenant";

        // Seed Tenant Demo - Usando constructor con validaciones de dominio
        var demoTenant = new Tenant(
            name: "Tenant Demo",
            tenantId: demoTenantId,
            createdBy: null
        );
        demoTenant.Id = 1;
        demoTenant.Description = "Tenant de demostración para pruebas";
        demoTenant.SetCreated();

        modelBuilder.Entity<Tenant>().HasData(demoTenant);

        // Seed Roles - Usando constructores con validaciones de dominio
        var adminRole = new Role("Admin", demoTenantId);
        adminRole.Id = 1;
        adminRole.Description = "Administrador del sistema";
        adminRole.SetCreated();

        var userRole = new Role("User", demoTenantId);
        userRole.Id = 2;
        userRole.Description = "Usuario estándar";
        userRole.SetCreated();

        var managerRole = new Role("Manager", demoTenantId);
        managerRole.Id = 3;
        managerRole.Description = "Gerente";
        managerRole.SetCreated();

        modelBuilder.Entity<Role>().HasData(adminRole, userRole, managerRole);

        // Seed Admin User
        // Contraseña hasheada con BCrypt para "Admin123!"
        // Hash generado con BCrypt.Net (workFactor: 12)
        // Para verificar: BCrypt.Net.BCrypt.Verify("Admin123!", hash)
        var adminUser = new User(
            username: "admin",
            email: "admin@demo.com",
            passwordHash: "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYqJqZqZqZq",
            tenantId: demoTenantId
        );
        adminUser.Id = 1;
        adminUser.FirstName = "Admin";
        adminUser.LastName = "User";
        adminUser.SetCreated();

        modelBuilder.Entity<User>().HasData(adminUser);

        // Seed UserRole (Admin tiene rol Admin)
        modelBuilder.Entity<UserRole>().HasData(
            new UserRole
            {
                Id = 1,
                UserId = 1,
                RoleId = 1, // Admin role
                TenantId = demoTenantId,
                AssignedAt = DateTime.UtcNow
            }
        );
    }

    public override int SaveChanges()
    {
        // Asegurar que todas las entidades ITenantEntity tengan TenantId antes de guardar
        EnsureTenantId();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Asegurar que todas las entidades ITenantEntity tengan TenantId antes de guardar
        EnsureTenantId();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Asegura que todas las entidades nuevas o modificadas tengan el TenantId asignado
    /// </summary>
    private void EnsureTenantId()
    {
        var tenantId = _tenantProvider.GetTenantId();

        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is ITenantEntity && 
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            if (entry.Entity is ITenantEntity tenantEntity)
            {
                if (string.IsNullOrEmpty(tenantEntity.TenantId))
                {
                    tenantEntity.TenantId = tenantId;
                }
            }
        }
    }
}

