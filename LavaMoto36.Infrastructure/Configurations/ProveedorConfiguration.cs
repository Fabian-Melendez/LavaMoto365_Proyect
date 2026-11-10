using LavaMoto365.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LavaMoto365.Infrastructure.Configurations;

public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> e)
    {
        e.ToTable("Proveedores");
        e.HasIndex(x => x.Nit).IsUnique();                       // NIT único
        e.Property(x => x.Nit).HasMaxLength(20).IsRequired();
        e.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        e.Property(x => x.Correo).HasMaxLength(150).IsRequired();
        e.Property(x => x.Presupuesto).HasPrecision(18, 2);
        e.Property(x => x.Estado).HasConversion<string>().HasMaxLength(10);
    }
}
