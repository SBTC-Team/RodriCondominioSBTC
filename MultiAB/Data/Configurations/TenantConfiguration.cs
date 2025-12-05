using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiAB.Models;

namespace MultiAB.Data.Configurations;

/// <summary>
/// Configuración de Fluent API para la entidad Tenant usando IEntityTypeConfiguration
/// </summary>
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        // Tabla
        builder.ToTable("Tenants");

        // Primary Key
        builder.HasKey(t => t.Id);

        // Propiedades
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnType("varchar(200)");

        builder.Property(t => t.Description)
            .HasMaxLength(500)
            .HasColumnType("varchar(500)");

        builder.Property(t => t.TenantId)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("varchar(50)");

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .ValueGeneratedNever();

        // Propiedades de AuditableEntity
        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.CreatedBy)
            .HasMaxLength(100)
            .HasColumnType("varchar(100)");

        builder.Property(t => t.UpdatedAt)
            .HasColumnType("datetime(6)");

        builder.Property(t => t.UpdatedBy)
            .HasMaxLength(100)
            .HasColumnType("varchar(100)");

        // Índices
        builder.HasIndex(t => t.TenantId)
            .IsUnique()
            .HasDatabaseName("IX_Tenants_TenantId");

        builder.HasIndex(t => t.Name)
            .HasDatabaseName("IX_Tenants_Name");

        // Filtro global multi-tenant (Tenant se filtra por su propio TenantId)
        builder.HasQueryFilter(t => t.TenantId == Services.TenantContext.CurrentTenantId);
    }
}

