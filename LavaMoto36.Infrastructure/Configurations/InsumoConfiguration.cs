using LavaMoto365.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LavaMoto365.Infrastructure.Configurations;

public class InsumoConfiguration : IEntityTypeConfiguration<Insumo>
{
    public void Configure(EntityTypeBuilder<Insumo> e)
    {
        e.ToTable("Insumos");
        e.HasIndex(x => x.Codigo).IsUnique();                    // código único
        e.Property(x => x.Codigo).HasMaxLength(30).IsRequired();
        e.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        e.Property(x => x.Categoria).HasMaxLength(80).IsRequired();
        e.Property(x => x.UnidadMedida).HasMaxLength(20).IsRequired();

        e.HasMany(x => x.Movimientos)
         .WithOne(m => m.Insumo)
         .HasForeignKey(m => m.InsumoId)
         .OnDelete(DeleteBehavior.Cascade);
        e.Navigation(x => x.Movimientos).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class MovimientoInventarioConfiguration : IEntityTypeConfiguration<MovimientoInventario>
{
    public void Configure(EntityTypeBuilder<MovimientoInventario> e)
    {
        e.ToTable("MovimientosInventario");
        e.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(10);
        e.Property(x => x.Motivo).HasMaxLength(200);
    }
}
