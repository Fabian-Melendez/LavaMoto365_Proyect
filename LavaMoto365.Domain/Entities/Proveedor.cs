using System.Text.RegularExpressions;
using LavaMoto365.Domain.Exceptions;

namespace LavaMoto365.Domain.Entities;

public enum EstadoProveedor { Activo, Inactivo }

/// <summary>Gestión de Proveedores (RF-PRO-001 a RF-PRO-003).</summary>
public class Proveedor
{
    private static readonly Regex CorreoRegex = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);

    public int Id { get; private set; }
    public string Nit { get; private set; } = "";
    public string Nombre { get; private set; } = "";
    public string Correo { get; private set; } = "";
    public decimal Presupuesto { get; private set; }
    public EstadoProveedor Estado { get; private set; }

    public bool EstaActivo => Estado == EstadoProveedor.Activo;

    private Proveedor() { } // requerido por EF Core

    public static Proveedor Crear(string nit, string nombre, string correo, decimal presupuesto)
    {
        if (string.IsNullOrWhiteSpace(nit)) throw new ReglaDeNegocioException("El NIT es obligatorio");
        ValidarDatos(nombre, correo, presupuesto);

        return new Proveedor
        {
            Nit = nit.Trim(),
            Nombre = nombre.Trim(),
            Correo = correo.Trim(),
            Presupuesto = presupuesto,
            Estado = EstadoProveedor.Activo
        };
    }

    /// <summary>Actualiza los datos editables (el NIT no se cambia).</summary>
    public void Actualizar(string nombre, string correo, decimal presupuesto)
    {
        ValidarDatos(nombre, correo, presupuesto);
        Nombre = nombre.Trim();
        Correo = correo.Trim();
        Presupuesto = presupuesto;
    }

    /// <summary>Eliminación lógica: el registro se conserva con estado Inactivo.</summary>
    public void Desactivar(bool tieneOrdenesActivas)
    {
        if (tieneOrdenesActivas) throw new ProveedorConOrdenesActivasException();
        Estado = EstadoProveedor.Inactivo;
    }

    public void Reactivar() => Estado = EstadoProveedor.Activo;

    /// <summary>Lo usa la aprobación de una orden de compra.</summary>
    public void DescontarPresupuesto(decimal monto)
    {
        if (monto <= 0) throw new ReglaDeNegocioException("El monto debe ser mayor que cero");
        if (monto > Presupuesto) throw new PresupuestoExcedidoException();
        Presupuesto -= monto;
    }

    private static void ValidarDatos(string nombre, string correo, decimal presupuesto)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ReglaDeNegocioException("El nombre es obligatorio");
        if (string.IsNullOrWhiteSpace(correo) || !CorreoRegex.IsMatch(correo.Trim()))
            throw new ReglaDeNegocioException("Formato de correo inválido");
        if (presupuesto < 0) throw new ReglaDeNegocioException("El presupuesto no puede ser negativo");
    }
}
