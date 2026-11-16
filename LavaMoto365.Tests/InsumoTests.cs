using LavaMoto365.Domain.Entities;
using LavaMoto365.Domain.Exceptions;
using Xunit;

namespace LavaMoto365.Tests;

public class InsumoTests
{
    // Casos del Scenario Outline "Retiro de inventario" del taller (Figura 24)
    [Theory]
    [InlineData("PROD-001", 10, 3, 7)]
    [InlineData("PROD-002", 5, 5, 0)]
    public void Retirar_DentroDelStock_DescuentaCantidad(string codigo, int stock, int retiro, int stockFinal)
    {
        var i = Insumo.Crear(codigo, "Shampoo", "Limpieza", stock);
        i.Retirar(retiro);
        Assert.Equal(stockFinal, i.Stock);
    }

    [Fact]
    public void Retirar_MayorAlStock_LanzaStockInsuficiente_YElStockNoCambia()
    {
        var i = Insumo.Crear("PROD-003", "Cera", "Limpieza", 2);

        var ex = Assert.Throws<StockInsuficienteException>(() => i.Retirar(5));

        Assert.Equal("Stock insuficiente", ex.Message);
        Assert.Equal(2, i.Stock);
    }

    [Fact]
    public void Retirar_Fallido_NoRegistraMovimiento()
    {
        var i = Insumo.Crear("PROD-003", "Cera", "Limpieza", 2);       // 1 movimiento: stock inicial
        Assert.Throws<StockInsuficienteException>(() => i.Retirar(5));
        Assert.Single(i.Movimientos);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Retirar_ConCantidadNoPositiva_Lanza(int cantidad)
    {
        var i = Insumo.Crear("PROD-001", "Shampoo", "Limpieza", 10);
        Assert.Throws<ReglaDeNegocioException>(() => i.Retirar(cantidad));
        Assert.Equal(10, i.Stock);
    }

    [Fact]
    public void Ingresar_SumaAlStock()
    {
        var i = Insumo.Crear("PROD-001", "Shampoo", "Limpieza", 10);
        i.Ingresar(5);
        Assert.Equal(15, i.Stock);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Ingresar_ConCantidadNoPositiva_Lanza(int cantidad)
    {
        var i = Insumo.Crear("PROD-001", "Shampoo", "Limpieza", 10);
        Assert.Throws<ReglaDeNegocioException>(() => i.Ingresar(cantidad));
    }

    [Fact]
    public void Movimientos_QuedanRegistradosConSuStockResultante()
    {
        var i = Insumo.Crear("PROD-001", "Shampoo", "Limpieza", 10);   // Entrada 10 (stock inicial)
        i.Retirar(3, "Lavado de motos");                                // Salida 3 -> 7
        i.Ingresar(5, "Compra OC-1");                                   // Entrada 5 -> 12

        var movs = i.Movimientos.ToList();
        Assert.Equal(3, movs.Count);
        Assert.Equal(TipoMovimiento.Salida, movs[1].Tipo);
        Assert.Equal(7, movs[1].StockResultante);
        Assert.Equal("Lavado de motos", movs[1].Motivo);
        Assert.Equal(12, movs[2].StockResultante);
    }

    [Fact]
    public void Crear_ConStockInicialCero_NoRegistraMovimiento()
    {
        var i = Insumo.Crear("PROD-001", "Shampoo", "Limpieza", 0);
        Assert.Empty(i.Movimientos);
    }

    [Fact]
    public void Crear_SinCategoria_Lanza()
    {
        var ex = Assert.Throws<ReglaDeNegocioException>(() => Insumo.Crear("P-1", "X", "", 1));
        Assert.Equal("La categoría es obligatoria", ex.Message);
    }

    [Fact]
    public void Crear_SinCodigo_Lanza()
        => Assert.Throws<ReglaDeNegocioException>(() => Insumo.Crear("", "X", "Cat", 1));

    [Fact]
    public void Crear_ConStockInicialNegativo_Lanza()
    {
        var ex = Assert.Throws<ReglaDeNegocioException>(() => Insumo.Crear("P-1", "X", "Cat", -1));
        Assert.Equal("El stock inicial debe ser mayor o igual a cero", ex.Message);
    }

    [Fact]
    public void Actualizar_CambiaNombreCategoriaYUnidad_PeroNoElCodigoNiElStock()
    {
        var i = Insumo.Crear("P-1", "Shampoo", "Limpieza", 10);
        i.Actualizar("Shampoo premium", "Químicos", "litros");
        Assert.Equal("Shampoo premium", i.Nombre);
        Assert.Equal("Químicos", i.Categoria);
        Assert.Equal("litros", i.UnidadMedida);
        Assert.Equal("P-1", i.Codigo);
        Assert.Equal(10, i.Stock);
    }
}
