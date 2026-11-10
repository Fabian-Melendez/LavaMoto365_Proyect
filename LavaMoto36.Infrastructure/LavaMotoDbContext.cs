using LavaMoto365.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LavaMoto365.Infrastructure;

/// <summary>
/// Contexto de los módulos de negocio (Identity tiene el suyo en el proyecto web).
/// Cada módulo tiene su propia clase en /Configurations, así Tania y Daniel
/// casi no tocan el mismo archivo: solo agregan su línea de DbSet.
/// </summary>
public class LavaMotoDbContext : DbContext
{
    public LavaMotoDbContext(DbContextOptions<LavaMotoDbContext> options) : base(options) { }

    // --- Tania ---
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<OrdenDeCompra> Ordenes => Set<OrdenDeCompra>();

    // --- Daniel: agregar aquí su DbSet<Insumo> ---

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(LavaMotoDbContext).Assembly);
}
