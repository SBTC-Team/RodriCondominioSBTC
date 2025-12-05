using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiAB.Models;

namespace MultiAB.Data.Configurations;

/// <summary>
/// Configuración de Fluent API para la entidad User usando IEntityTypeConfiguration
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Tabla
        builder.ToTable("Users");

        // Primary Key
        builder.HasKey(u => u.Id);

        // Configurar propiedades privadas para EF Core
        builder.Property(u => u.Id)
            .ValueGeneratedOnAdd();

        // Propiedades con tipos VARCHAR específicos
        // EF Core trabajará con los setters públicos que tienen validaciones
        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnType("varchar(100)");

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnType("varchar(255)");

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnType("varchar(500)");

        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .HasColumnType("varchar(100)");

        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .HasColumnType("varchar(100)");

        builder.Property(u => u.TenantId)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("varchar(50)");

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .ValueGeneratedNever(); // Forzar que EF Core siempre incluya el valor en INSERT

        // Propiedades de AuditableEntity
        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.CreatedBy)
            .HasMaxLength(100)
            .HasColumnType("varchar(100)");

        builder.Property(u => u.UpdatedAt)
            .HasColumnType("datetime(6)");

        builder.Property(u => u.UpdatedBy)
            .HasMaxLength(100)
            .HasColumnType("varchar(100)");

        // Índices
        builder.HasIndex(u => new { u.Username, u.TenantId })
            .IsUnique()
            .HasDatabaseName("IX_Users_Username_TenantId");

        builder.HasIndex(u => new { u.Email, u.TenantId })
            .IsUnique()
            .HasDatabaseName("IX_Users_Email_TenantId");

        builder.HasIndex(u => u.TenantId)
            .HasDatabaseName("IX_Users_TenantId");

        // Filtro global multi-tenant
        builder.HasQueryFilter(u => u.TenantId == Services.TenantContext.CurrentTenantId);
    }
}

