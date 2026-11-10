namespace LavaMoto365.Domain.Exceptions;

public class ProveedorConOrdenesActivasException : ReglaDeNegocioException
{
    public ProveedorConOrdenesActivasException()
        : base("No se puede desactivar un proveedor con órdenes de compra activas") { }
}

public class PresupuestoExcedidoException : ReglaDeNegocioException
{
    public PresupuestoExcedidoException()
        : base("La orden excede el presupuesto del proveedor") { }
}

public class OrdenYaAprobadaException : ReglaDeNegocioException
{
    public OrdenYaAprobadaException()
        : base("La orden ya fue aprobada") { }
}
