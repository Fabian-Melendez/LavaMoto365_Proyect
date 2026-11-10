using LavaMoto365.Domain.Entities;
using LavaMoto365.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LavaMoto365.Infrastructure.Services;

public class OrdenCompraService
{
    private readonly IDbContextFactory<LavaMotoDbContext> _factory;
    public OrdenCompraService(IDbContextFactory<LavaMotoDbContext> factory) => _factory = factory;

    public async Task<List<OrdenDeCompra>> ListarAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Ordenes
            .Include(o => o.Proveedor)
            .Include(o => o.Detalles)
            .AsNoTracking()
            .OrderByDescending(o => o.Fecha)
            .ToListAsync();
    }

    public async Task<OrdenDeCompra> CrearAsync(
        int proveedorId,
        IEnumerable<(string Descripcion, int Cantidad, decimal PrecioUnitario)> items)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var proveedor = await db.Proveedores.FindAsync(proveedorId);   // null => la regla del dominio lo rechaza
        var orden = OrdenDeCompra.Crear(proveedor!, items);
        db.Ordenes.Add(orden);
        await db.SaveChangesAsync();
        return orden;
    }

    public async Task AprobarAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var orden = await CargarAsync(db, id);
        orden.Aprobar();                 // descuenta el presupuesto del proveedor (misma transacción)
        await db.SaveChangesAsync();
    }

    public async Task CancelarAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var orden = await CargarAsync(db, id);
        orden.Cancelar();
        await db.SaveChangesAsync();
    }

    public async Task MarcarRecibidaAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var orden = await CargarAsync(db, id);
        orden.MarcarRecibida();
        await db.SaveChangesAsync();
    }

    private static async Task<OrdenDeCompra> CargarAsync(LavaMotoDbContext db, int id) =>
        await db.Ordenes
            .Include(o => o.Proveedor)
            .Include(o => o.Detalles)
            .FirstOrDefaultAsync(o => o.Id == id)
        ?? throw new ReglaDeNegocioException("La orden no existe");
}
