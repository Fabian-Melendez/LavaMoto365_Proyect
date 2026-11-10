using LavaMoto365.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LavaMoto365.Infrastructure.Configurations;

public class OrdenDeCompraConfiguration : IEntityTypeConfiguration<OrdenDeCompra>
{
    public void Configure(EntityTypeBuilder<OrdenDeCompra> e)
    {
        e.ToTable("OrdenesDeCompra");
        e.Property(x => x.Estado).HasConversion<string>().HasMaxLength(12);

        e.HasOne(x => x.Proveedor)
         .WithMany()
         .HasForeignKey(x => x.ProveedorId)
         .OnDelete(DeleteBehavior.Restrict);                      // no borrar proveedor con órdenes

        e.HasMany(x => x.Detalles)
         .WithOne()
         .HasForeignKey(d => d.OrdenDeCompraId)
         .OnDelete(DeleteBehavior.Cascade);
        e.Navigation(x => x.Detalles).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class DetalleOrdenConfiguration : IEntityTypeConfiguration<DetalleOrden>
{
    public void Configure(EntityTypeBuilder<DetalleOrden> e)
    {
        e.ToTable("DetallesOrden");
        e.Property(x => x.Descripcion).HasMaxLength(200).IsRequired();
        e.Property(x => x.PrecioUnitario).HasPrecision(18, 2);
    }
}
