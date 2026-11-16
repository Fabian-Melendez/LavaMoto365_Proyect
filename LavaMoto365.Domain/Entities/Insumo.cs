using LavaMoto365.Domain.Exceptions;

namespace LavaMoto365.Domain.Entities;

public enum TipoMovimiento { Entrada, Salida }

/// <summary>Registro de cada entrada o salida de un insumo (RF-INV-002).</summary>
public class MovimientoInventario
{
    public int Id { get; private set; }
    public int InsumoId { get; private set; }
    public Insumo Insumo { get; private set; } = null!;
    public TipoMovimiento Tipo { get; private set; }
    public int Cantidad { get; private set; }
    public int StockResultante { get; private set; }
    public DateTime Fecha { get; private set; }
    public string Motivo { get; private set; } = "";

    private MovimientoInventario() { } // EF Core

    internal static MovimientoInventario Crear(Insumo insumo, TipoMovimiento tipo, int cantidad, int stockResultante, string? motivo)
        => new()
        {
            Insumo = insumo,
            Tipo = tipo,
            Cantidad = cantidad,
            StockResultante = stockResultante,
            Fecha = DateTime.Now,
            Motivo = (motivo ?? "").Trim()
        };
}

/// <summary>Producto, materia prima o insumo del Inventario y Almacén (RF-INV-001 y RF-INV-002).</summary>
public class Insumo
{
    private readonly List<MovimientoInventario> _movimientos = new();

    public int Id { get; private set; }
    public string Codigo { get; private set; } = "";
    public string Nombre { get; private set; } = "";
    public string Categoria { get; private set; } = "";
    public string UnidadMedida { get; private set; } = "und";
    public int Stock { get; private set; }
    public IReadOnlyCollection<MovimientoInventario> Movimientos => _movimientos;

    private Insumo() { } // EF Core

    public static Insumo Crear(string codigo, string nombre, string categoria, int stockInicial, string unidadMedida = "und")
    {
        if (string.IsNullOrWhiteSpace(codigo)) throw new ReglaDeNegocioException("El código es obligatorio");
        ValidarDatos(nombre, categoria);
        if (stockInicial < 0) throw new ReglaDeNegocioException("El stock inicial debe ser mayor o igual a cero");

        var insumo = new Insumo
        {
            Codigo = codigo.Trim(),
            Nombre = nombre.Trim(),
            Categoria = categoria.Trim(),
            UnidadMedida = string.IsNullOrWhiteSpace(unidadMedida) ? "und" : unidadMedida.Trim(),
            Stock = stockInicial
        };

        if (stockInicial > 0) insumo.Registrar(TipoMovimiento.Entrada, stockInicial, "Stock inicial");
        return insumo;
    }

    public void Actualizar(string nombre, string categoria, string unidadMedida)
    {
        ValidarDatos(nombre, categoria);
        Nombre = nombre.Trim();
        Categoria = categoria.Trim();
        UnidadMedida = string.IsNullOrWhiteSpace(unidadMedida) ? "und" : unidadMedida.Trim();
    }

    public void Ingresar(int cantidad, string? motivo = null)
    {
        if (cantidad <= 0) throw new ReglaDeNegocioException("La cantidad debe ser mayor que cero");
        Stock += cantidad;
        Registrar(TipoMovimiento.Entrada, cantidad, string.IsNullOrWhiteSpace(motivo) ? "Ingreso" : motivo);
    }

    /// <summary>Retira unidades. Si no alcanza el stock, lanza StockInsuficienteException y NO modifica nada.</summary>
    public void Retirar(int cantidad, string? motivo = null)
    {
        if (cantidad <= 0) throw new ReglaDeNegocioException("La cantidad debe ser mayor que cero");
        if (cantidad > Stock) throw new StockInsuficienteException();
        Stock -= cantidad;
        Registrar(TipoMovimiento.Salida, cantidad, string.IsNullOrWhiteSpace(motivo) ? "Retiro" : motivo);
    }

    private void Registrar(TipoMovimiento tipo, int cantidad, string? motivo)
        => _movimientos.Add(MovimientoInventario.Crear(this, tipo, cantidad, Stock, motivo));

    private static void ValidarDatos(string nombre, string categoria)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ReglaDeNegocioException("El nombre es obligatorio");
        if (string.IsNullOrWhiteSpace(categoria)) throw new ReglaDeNegocioException("La categoría es obligatoria");
    }
}
