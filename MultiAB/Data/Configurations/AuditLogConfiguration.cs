using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiAB.Models;

namespace MultiAB.Data.Configurations;

/// <summary>
/// Configuración de Fluent API para la entidad AuditLog usando IEntityTypeConfiguration
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        // Tabla
        builder.ToTable("AuditLogs");

        // Primary Key
        builder.HasKey(a => a.Id);

        // Propiedades con tipos VARCHAR específicos
        builder.Property(a => a.EntityName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnType("varchar(100)");

        builder.Property(a => a.EntityId)
            .HasMaxLength(50)
            .HasColumnType("varchar(50)");

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("varchar(50)");

        builder.Property(a => a.UserId)
            .HasMaxLength(100)
            .HasColumnType("varchar(100)");

        builder.Property(a => a.Description)
            .HasMaxLength(500)
            .HasColumnType("varchar(500)");

        builder.Property(a => a.Changes)
            .HasColumnType("TEXT");

        builder.Property(a => a.IpAddress)
            .HasMaxLength(45)
            .HasColumnType("varchar(45)");

        builder.Property(a => a.TenantId)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("varchar(50)");

        // Propiedades de AuditableEntity
        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.CreatedBy)
            .HasMaxLength(100)
            .HasColumnType("varchar(100)");

        builder.Property(a => a.UpdatedAt)
            .HasColumnType("datetime(6)");

        builder.Property(a => a.UpdatedBy)
            .HasMaxLength(100)
            .HasColumnType("varchar(100)");

        // Índices
        builder.HasIndex(a => a.TenantId)
            .HasDatabaseName("IX_AuditLogs_TenantId");

        builder.HasIndex(a => new { a.EntityName, a.EntityId })
            .HasDatabaseName("IX_AuditLogs_EntityName_EntityId");

        builder.HasIndex(a => a.CreatedAt)
            .HasDatabaseName("IX_AuditLogs_CreatedAt");

        // Filtro global multi-tenant
        builder.HasQueryFilter(a => a.TenantId == Services.TenantContext.CurrentTenantId);
    }
}

