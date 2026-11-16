using LavaMoto365.Domain.Entities;
using LavaMoto365.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LavaMoto365.Infrastructure.Services;

public class InventarioService
{
    private readonly IDbContextFactory<LavaMotoDbContext> _factory;
    public InventarioService(IDbContextFactory<LavaMotoDbContext> factory) => _factory = factory;

    public async Task<List<Insumo>> ListarAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Insumos.AsNoTracking().OrderBy(i => i.Codigo).ToListAsync();
    }

    public async Task<List<MovimientoInventario>> ListarMovimientosAsync(int maximo = 30)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Movimientos
            .Include(m => m.Insumo)
            .AsNoTracking()
            .OrderByDescending(m => m.Fecha).ThenByDescending(m => m.Id)
            .Take(maximo)
            .ToListAsync();
    }

    public async Task<Insumo> RegistrarAsync(string codigo, string nombre, string categoria, int stockInicial, string unidad = "und")
    {
        await using var db = await _factory.CreateDbContextAsync();
        var codigoLimpio = (codigo ?? "").Trim();
        if (await db.Insumos.AnyAsync(i => i.Codigo == codigoLimpio))
            throw new ReglaDeNegocioException("El código del producto ya existe");

        var insumo = Insumo.Crear(codigo!, nombre, categoria, stockInicial, unidad);
        db.Insumos.Add(insumo);
        await db.SaveChangesAsync();
        return insumo;
    }

    public async Task ActualizarAsync(int id, string nombre, string categoria, string unidad)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var insumo = await db.Insumos.FindAsync(id) ?? throw new ReglaDeNegocioException("El insumo no existe");
        insumo.Actualizar(nombre, categoria, unidad);
        await db.SaveChangesAsync();
    }

    public async Task IngresarAsync(int id, int cantidad, string? motivo = null)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var insumo = await db.Insumos.FindAsync(id) ?? throw new ReglaDeNegocioException("El insumo no existe");
        insumo.Ingresar(cantidad, motivo);
        await db.SaveChangesAsync();
    }

    public async Task RetirarAsync(int id, int cantidad, string? motivo = null)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var insumo = await db.Insumos.FindAsync(id) ?? throw new ReglaDeNegocioException("El insumo no existe");
        insumo.Retirar(cantidad, motivo);   // lanza StockInsuficienteException sin modificar el stock
        await db.SaveChangesAsync();
    }
}
