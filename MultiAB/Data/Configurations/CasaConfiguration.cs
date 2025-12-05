using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiAB.Models;

namespace MultiAB.Data.Configurations;

/// <summary>
/// Configuración de Fluent API para la entidad Casa usando IEntityTypeConfiguration
/// </summary>
public class CasaConfiguration : IEntityTypeConfiguration<Casa>
{
    public void Configure(EntityTypeBuilder<Casa> builder)
    {
        // Tabla
        builder.ToTable("Casas");

        // Primary Key
        builder.HasKey(c => c.Id);

        // Propiedades con tipos VARCHAR específicos
        builder.Property(c => c.NumeroCasa)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("varchar(50)");

        builder.Property(c => c.Descripcion)
            .HasMaxLength(200)
            .HasColumnType("varchar(200)");

        builder.Property(c => c.Tipo)
            .HasMaxLength(50)
            .HasColumnType("varchar(50)");

        builder.Property(c => c.AreaMetros)
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.TenantId)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("varchar(50)");

        builder.Property(c => c.NumeroHabitaciones);

        builder.Property(c => c.NumeroBanos);

        builder.Property(c => c.TieneGaraje)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.IsOcupada)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        // Relación con Condominio
        builder.HasOne(c => c.Condominio)
            .WithMany(co => co.Casas)
            .HasForeignKey(c => c.CondominioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(c => new { c.NumeroCasa, c.CondominioId, c.TenantId })
            .IsUnique()
            .HasDatabaseName("IX_Casas_NumeroCasa_CondominioId_TenantId");

        builder.HasIndex(c => c.TenantId)
            .HasDatabaseName("IX_Casas_TenantId");

        builder.HasIndex(c => c.CondominioId)
            .HasDatabaseName("IX_Casas_CondominioId");

        builder.HasIndex(c => c.IsOcupada)
            .HasDatabaseName("IX_Casas_IsOcupada");

        // Filtro global multi-tenant
        builder.HasQueryFilter(c => c.TenantId == Services.TenantContext.CurrentTenantId);
    }
}

