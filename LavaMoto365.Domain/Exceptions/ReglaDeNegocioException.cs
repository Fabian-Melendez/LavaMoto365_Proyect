namespace LavaMoto365.Domain.Exceptions;

/// <summary>
/// Excepción base para cualquier regla de negocio violada.
/// OJO: este archivo lo comparte todo el equipo (Daniel hará StockInsuficienteException sobre esta base).
/// El primero que lo suba a GitHub gana; el otro solo hace pull y no lo vuelve a crear.
/// </summary>
public class ReglaDeNegocioException : Exception
{
    public ReglaDeNegocioException(string mensaje) : base(mensaje) { }
}
