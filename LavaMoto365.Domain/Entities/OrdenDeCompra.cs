using LavaMoto365.Domain.Exceptions;

namespace LavaMoto365.Domain.Entities;

public enum EstadoOrden { Pendiente, Aprobada, Recibida, Cancelada }

/// <summary>Línea de una orden de compra.</summary>
public class DetalleOrden
{
    public int Id { get; private set; }
    public int OrdenDeCompraId { get; private set; }
    public string Descripcion { get; private set; } = "";
    public int Cantidad { get; private set; }
    public decimal PrecioUnitario { get; private set; }

    public decimal Subtotal => Cantidad * PrecioUnitario;

    private DetalleOrden() { } // EF Core

    internal static DetalleOrden Crear(string descripcion, int cantidad, decimal precioUnitario)
    {
        if (string.IsNullOrWhiteSpace(descripcion)) throw new ReglaDeNegocioException("La descripción del ítem es obligatoria");
        if (cantidad <= 0) throw new ReglaDeNegocioException("La cantidad debe ser mayor que cero");
        if (precioUnitario <= 0) throw new ReglaDeNegocioException("El precio unitario debe ser mayor que cero");
        return new DetalleOrden { Descripcion = descripcion.Trim(), Cantidad = cantidad, PrecioUnitario = precioUnitario };
    }
}

/// <summary>Orden de compra a un proveedor (RF-PRO-004).</summary>
public class OrdenDeCompra
{
    private readonly List<DetalleOrden> _detalles = new();

    public int Id { get; private set; }
    public int ProveedorId { get; private set; }
    public Proveedor Proveedor { get; private set; } = null!;
    public DateTime Fecha { get; private set; }
    public EstadoOrden Estado { get; private set; }
    public IReadOnlyCollection<DetalleOrden> Detalles => _detalles;

    public decimal Total => _detalles.Sum(d => d.Subtotal);

    /// <summary>Una orden está "activa" mientras no se haya recibido ni cancelado.</summary>
    public bool EstaActiva => Estado == EstadoOrden.Pendiente || Estado == EstadoOrden.Aprobada;

    private OrdenDeCompra() { } // EF Core

    public static OrdenDeCompra Crear(
        Proveedor proveedor,
        IEnumerable<(string Descripcion, int Cantidad, decimal PrecioUnitario)> items,
        DateTime? fecha = null)
    {
        if (proveedor is null || !proveedor.EstaActivo)
            throw new ReglaDeNegocioException("El proveedor no existe o no está activo");

        var lista = items?.ToList() ?? new();
        if (lista.Count == 0)
            throw new ReglaDeNegocioException("La orden debe tener al menos un ítem");

        var orden = new OrdenDeCompra
        {
            Proveedor = proveedor,
            ProveedorId = proveedor.Id,
            Fecha = fecha ?? DateTime.Now,
            Estado = EstadoOrden.Pendiente
        };

        foreach (var i in lista)
            orden._detalles.Add(DetalleOrden.Crear(i.Descripcion, i.Cantidad, i.PrecioUnitario));

        return orden;
    }

    /// <summary>Aprueba la orden y descuenta su total del presupuesto del proveedor.</summary>
    public void Aprobar()
    {
        if (Estado == EstadoOrden.Aprobada) throw new OrdenYaAprobadaException();
        if (Estado != EstadoOrden.Pendiente)
            throw new ReglaDeNegocioException("Solo se pueden aprobar órdenes pendientes");

        Proveedor.DescontarPresupuesto(Total); // lanza PresupuestoExcedidoException si no alcanza
        Estado = EstadoOrden.Aprobada;
    }

    public void Cancelar()
    {
        if (Estado != EstadoOrden.Pendiente)
            throw new ReglaDeNegocioException("Solo se pueden cancelar órdenes pendientes");
        Estado = EstadoOrden.Cancelada;
    }

    public void MarcarRecibida()
    {
        if (Estado != EstadoOrden.Aprobada)
            throw new ReglaDeNegocioException("Solo se pueden recibir órdenes aprobadas");
        Estado = EstadoOrden.Recibida;
    }
}
