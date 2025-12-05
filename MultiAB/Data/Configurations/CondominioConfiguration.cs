using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiAB.Models;

namespace MultiAB.Data.Configurations;

/// <summary>
/// Configuración de Fluent API para la entidad Condominio usando IEntityTypeConfiguration
/// </summary>
public class CondominioConfiguration : IEntityTypeConfiguration<Condominio>
{
    public void Configure(EntityTypeBuilder<Condominio> builder)
    {
        // Tabla
        builder.ToTable("Condominios");

        // Primary Key
        builder.HasKey(c => c.Id);

        // Propiedades con tipos VARCHAR específicos
        builder.Property(c => c.Nombre)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnType("varchar(200)");

        builder.Property(c => c.Direccion)
            .HasMaxLength(500)
            .HasColumnType("varchar(500)");

        builder.Property(c => c.Ciudad)
            .HasMaxLength(100)
            .HasColumnType("varchar(100)");

        builder.Property(c => c.CodigoPostal)
            .HasMaxLength(50)
            .HasColumnType("varchar(50)");

        builder.Property(c => c.Telefono)
            .HasMaxLength(20)
            .HasColumnType("varchar(20)");

        builder.Property(c => c.Email)
            .HasMaxLength(255)
            .HasColumnType("varchar(255)");

        builder.Property(c => c.TenantId)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("varchar(50)");

        builder.Property(c => c.TotalCasas)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        // Índices
        builder.HasIndex(c => new { c.Nombre, c.TenantId })
            .IsUnique()
            .HasDatabaseName("IX_Condominios_Nombre_TenantId");

        builder.HasIndex(c => c.TenantId)
            .HasDatabaseName("IX_Condominios_TenantId");

        builder.HasIndex(c => c.Ciudad)
            .HasDatabaseName("IX_Condominios_Ciudad");

        // Filtro global multi-tenant
        builder.HasQueryFilter(c => c.TenantId == Services.TenantContext.CurrentTenantId);
    }
}

