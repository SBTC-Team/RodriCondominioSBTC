using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiAB.Models;

namespace MultiAB.Data.Configurations;

/// <summary>
/// Configuración de Fluent API para la entidad UserRole usando IEntityTypeConfiguration
/// </summary>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        // Tabla
        builder.ToTable("UserRoles");

        // Primary Key
        builder.HasKey(ur => ur.Id);

        // Propiedades
        builder.Property(ur => ur.UserId)
            .IsRequired();

        builder.Property(ur => ur.RoleId)
            .IsRequired();

        builder.Property(ur => ur.TenantId)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("varchar(50)");

        builder.Property(ur => ur.AssignedAt)
            .IsRequired();

        // Relaciones
        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(ur => ur.UserId)
            .HasDatabaseName("IX_UserRoles_UserId");

        builder.HasIndex(ur => ur.RoleId)
            .HasDatabaseName("IX_UserRoles_RoleId");

        builder.HasIndex(ur => new { ur.UserId, ur.RoleId, ur.TenantId })
            .IsUnique()
            .HasDatabaseName("IX_UserRoles_UserId_RoleId_TenantId");

        builder.HasIndex(ur => ur.TenantId)
            .HasDatabaseName("IX_UserRoles_TenantId");

        // Filtro global multi-tenant
        builder.HasQueryFilter(ur => ur.TenantId == Services.TenantContext.CurrentTenantId);
    }
}

