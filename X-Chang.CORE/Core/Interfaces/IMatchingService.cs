namespace X_Chang.CORE.Interfaces;

public interface IMatchingService
{
    Task EjecutarMatchingOrdenAsync(int ordenCompraId);
    Task EjecutarMatchingOfertaAsync(int ofertaVentaId);
}
