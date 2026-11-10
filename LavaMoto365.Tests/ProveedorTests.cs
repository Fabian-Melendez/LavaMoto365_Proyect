using LavaMoto365.Domain.Entities;
using LavaMoto365.Domain.Exceptions;
using Xunit;

namespace LavaMoto365.Tests;

public class ProveedorTests
{
    [Fact]
    public void Crear_ConDatosValidos_QuedaActivo()
    {
        var p = Proveedor.Crear("900123456", "ACME S.A.S.", "contacto@acme.com", 10_000_000);
        Assert.Equal(EstadoProveedor.Activo, p.Estado);
        Assert.Equal(10_000_000, p.Presupuesto);
    }

    [Theory]
    [InlineData("usuario@")]
    [InlineData("sin-arroba.com")]
    [InlineData("")]
    public void Crear_ConCorreoInvalido_Lanza(string correo)
    {
        var ex = Assert.Throws<ReglaDeNegocioException>(() => Proveedor.Crear("900999999", "Test", correo, 0));
        Assert.Equal("Formato de correo inválido", ex.Message);
    }

    [Fact]
    public void Crear_SinNombre_Lanza()
    {
        var ex = Assert.Throws<ReglaDeNegocioException>(() => Proveedor.Crear("900123456", " ", "a@b.com", 0));
        Assert.Equal("El nombre es obligatorio", ex.Message);
    }

    [Fact]
    public void Crear_SinNit_Lanza()
        => Assert.Throws<ReglaDeNegocioException>(() => Proveedor.Crear("", "ACME", "a@b.com", 0));

    [Fact]
    public void Desactivar_SinOrdenesActivas_PasaAInactivo()
    {
        var p = Proveedor.Crear("900123456", "ACME", "a@b.com", 0);
        p.Desactivar(tieneOrdenesActivas: false);
        Assert.Equal(EstadoProveedor.Inactivo, p.Estado);
    }

    [Fact]
    public void Desactivar_ConOrdenesActivas_Lanza_YNoCambiaEstado()
    {
        var p = Proveedor.Crear("900123456", "ACME", "a@b.com", 0);
        Assert.Throws<ProveedorConOrdenesActivasException>(() => p.Desactivar(true));
        Assert.Equal(EstadoProveedor.Activo, p.Estado);
    }

    [Fact]
    public void Reactivar_DejaElProveedorActivo()
    {
        var p = Proveedor.Crear("900123456", "ACME", "a@b.com", 0);
        p.Desactivar(false);
        p.Reactivar();
        Assert.True(p.EstaActivo);
    }

    [Fact]
    public void Actualizar_CambiaNombreCorreoYPresupuesto_PeroNoElNit()
    {
        var p = Proveedor.Crear("900123456", "ACME", "a@b.com", 100);
        p.Actualizar("ACME Nuevo", "nuevo@acme.com", 500);
        Assert.Equal("ACME Nuevo", p.Nombre);
        Assert.Equal("nuevo@acme.com", p.Correo);
        Assert.Equal(500, p.Presupuesto);
        Assert.Equal("900123456", p.Nit);
    }

    [Fact]
    public void DescontarPresupuesto_MayorAlDisponible_Lanza_YNoCambia()
    {
        var p = Proveedor.Crear("900123456", "ACME", "a@b.com", 1_000);
        Assert.Throws<PresupuestoExcedidoException>(() => p.DescontarPresupuesto(1_001));
        Assert.Equal(1_000, p.Presupuesto);
    }
}
