using LavaMoto365.Domain.Entities;
using LavaMoto365.Domain.Exceptions;
using Xunit;

namespace LavaMoto365.Tests;

public class OrdenDeCompraTests
{
    private static Proveedor ProveedorActivo(decimal presupuesto) =>
        Proveedor.Crear("900123456", "ACME S.A.S.", "contacto@acme.com", presupuesto);

    private static OrdenDeCompra OrdenDe(Proveedor p, int cantidad, decimal precio) =>
        OrdenDeCompra.Crear(p, new[] { ("Shampoo para motos", cantidad, precio) });

    [Fact]
    public void Crear_CalculaElTotalDeTodosLosItems()
    {
        var orden = OrdenDeCompra.Crear(ProveedorActivo(1_000_000), new[]
        {
            ("Shampoo", 2, 50_000m),   // 100.000
            ("Cera",    1, 20_000m)    //  20.000
        });

        Assert.Equal(120_000, orden.Total);
        Assert.Equal(EstadoOrden.Pendiente, orden.Estado);
        Assert.Equal(2, orden.Detalles.Count);
    }

    [Fact]
    public void Crear_ConProveedorInactivo_Lanza()
    {
        var p = ProveedorActivo(1_000_000);
        p.Desactivar(false);
        var ex = Assert.Throws<ReglaDeNegocioException>(() => OrdenDe(p, 1, 100));
        Assert.Equal("El proveedor no existe o no está activo", ex.Message);
    }

    [Fact]
    public void Crear_SinItems_Lanza()
        => Assert.Throws<ReglaDeNegocioException>(() =>
            OrdenDeCompra.Crear(ProveedorActivo(1_000), Array.Empty<(string, int, decimal)>()));

    [Theory]
    [InlineData(0, 100)]
    [InlineData(-1, 100)]
    [InlineData(1, 0)]
    [InlineData(1, -5)]
    public void Crear_ConCantidadOPrecioInvalidos_Lanza(int cantidad, decimal precio)
        => Assert.Throws<ReglaDeNegocioException>(() => OrdenDe(ProveedorActivo(1_000_000), cantidad, precio));

    [Fact]
    public void Aprobar_DentroDelPresupuesto_DescuentaElTotal()
    {
        var proveedor = ProveedorActivo(10_000_000);
        var orden = OrdenDe(proveedor, 5, 20_000);   // 100.000

        orden.Aprobar();

        Assert.Equal(EstadoOrden.Aprobada, orden.Estado);
        Assert.Equal(9_900_000, proveedor.Presupuesto);
    }

    [Fact]
    public void Aprobar_QueExcedeElPresupuesto_Lanza_YNoCambiaNada()
    {
        var proveedor = ProveedorActivo(50_000);
        var orden = OrdenDe(proveedor, 5, 20_000);   // 100.000 > 50.000

        Assert.Throws<PresupuestoExcedidoException>(() => orden.Aprobar());

        Assert.Equal(EstadoOrden.Pendiente, orden.Estado);
        Assert.Equal(50_000, proveedor.Presupuesto);
    }

    [Fact]
    public void Aprobar_UnaOrdenYaAprobada_Lanza_YNoDescuentaDeNuevo()
    {
        var proveedor = ProveedorActivo(1_000_000);
        var orden = OrdenDe(proveedor, 1, 100_000);
        orden.Aprobar();

        Assert.Throws<OrdenYaAprobadaException>(() => orden.Aprobar());

        Assert.Equal(900_000, proveedor.Presupuesto);
    }

    [Fact]
    public void Cancelar_UnaOrdenPendiente_LaDejaCancelada()
    {
        var orden = OrdenDe(ProveedorActivo(1_000_000), 1, 1_000);
        orden.Cancelar();
        Assert.Equal(EstadoOrden.Cancelada, orden.Estado);
        Assert.False(orden.EstaActiva);
    }

    [Fact]
    public void Cancelar_UnaOrdenAprobada_Lanza()
    {
        var orden = OrdenDe(ProveedorActivo(1_000_000), 1, 1_000);
        orden.Aprobar();
        Assert.Throws<ReglaDeNegocioException>(() => orden.Cancelar());
    }

    [Fact]
    public void MarcarRecibida_SoloSiEstaAprobada()
    {
        var orden = OrdenDe(ProveedorActivo(1_000_000), 1, 1_000);
        Assert.Throws<ReglaDeNegocioException>(() => orden.MarcarRecibida());

        orden.Aprobar();
        orden.MarcarRecibida();
        Assert.Equal(EstadoOrden.Recibida, orden.Estado);
    }
}
