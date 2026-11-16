namespace LavaMoto365.Domain.Exceptions;

/// <summary>Se lanza al intentar retirar más unidades de las disponibles. El stock no cambia.</summary>
public class StockInsuficienteException : ReglaDeNegocioException
{
    public StockInsuficienteException() : base("Stock insuficiente") { }
}
