using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiAB.Models;

namespace MultiAB.Data.Configurations;

/// <summary>
/// Configuración de Fluent API para la entidad Role usando IEntityTypeConfiguration
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        // Tabla
        builder.ToTable("Roles");

        // Primary Key
        builder.HasKey(r => r.Id);

        // Propiedades con tipos VARCHAR específicos
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnType("varchar(100)");

        builder.Property(r => r.Description)
            .HasMaxLength(500)
            .HasColumnType("varchar(500)");

        builder.Property(r => r.TenantId)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("varchar(50)");

        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Propiedades de AuditableEntity
        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.CreatedBy)
            .HasMaxLength(100)
            .HasColumnType("varchar(100)");

        builder.Property(r => r.UpdatedAt)
            .HasColumnType("datetime(6)");

        builder.Property(r => r.UpdatedBy)
            .HasMaxLength(100)
            .HasColumnType("varchar(100)");

        // Índices
        builder.HasIndex(r => new { r.Name, r.TenantId })
            .IsUnique()
            .HasDatabaseName("IX_Roles_Name_TenantId");

        builder.HasIndex(r => r.TenantId)
            .HasDatabaseName("IX_Roles_TenantId");

        // Filtro global multi-tenant
        builder.HasQueryFilter(r => r.TenantId == Services.TenantContext.CurrentTenantId);
    }
}

