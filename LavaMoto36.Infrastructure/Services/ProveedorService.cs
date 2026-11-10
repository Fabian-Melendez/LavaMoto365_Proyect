using LavaMoto365.Domain.Entities;
using LavaMoto365.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LavaMoto365.Infrastructure.Services;

public class ProveedorService
{
    private readonly IDbContextFactory<LavaMotoDbContext> _factory;
    public ProveedorService(IDbContextFactory<LavaMotoDbContext> factory) => _factory = factory;

    public async Task<List<Proveedor>> ListarAsync(bool soloActivos = false)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var q = db.Proveedores.AsNoTracking();
        if (soloActivos) q = q.Where(p => p.Estado == EstadoProveedor.Activo);
        return await q.OrderBy(p => p.Nombre).ToListAsync();
    }

    public async Task<Proveedor> CrearAsync(string nit, string nombre, string correo, decimal presupuesto)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var nitLimpio = (nit ?? "").Trim();
        if (await db.Proveedores.AnyAsync(p => p.Nit == nitLimpio))
            throw new ReglaDeNegocioException("El NIT ya está registrado");

        var proveedor = Proveedor.Crear(nit!, nombre, correo, presupuesto);
        db.Proveedores.Add(proveedor);
        await db.SaveChangesAsync();
        return proveedor;
    }

    public async Task ActualizarAsync(int id, string nombre, string correo, decimal presupuesto)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var proveedor = await db.Proveedores.FindAsync(id)
            ?? throw new ReglaDeNegocioException("El proveedor no existe");
        proveedor.Actualizar(nombre, correo, presupuesto);
        await db.SaveChangesAsync();
    }

    public async Task DesactivarAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var proveedor = await db.Proveedores.FindAsync(id)
            ?? throw new ReglaDeNegocioException("El proveedor no existe");

        bool tieneOrdenesActivas = await db.Ordenes.AnyAsync(o =>
            o.ProveedorId == id &&
            (o.Estado == EstadoOrden.Pendiente || o.Estado == EstadoOrden.Aprobada));

        proveedor.Desactivar(tieneOrdenesActivas);
        await db.SaveChangesAsync();
    }

    public async Task ReactivarAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var proveedor = await db.Proveedores.FindAsync(id)
            ?? throw new ReglaDeNegocioException("El proveedor no existe");
        proveedor.Reactivar();
        await db.SaveChangesAsync();
    }
}
