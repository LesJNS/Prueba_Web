using X_Chang.API.Models;

namespace X_Chang.CORE.Repositories;

public interface IRutaRepository
{
    Task<BusquedasRuta?> ObtenerConRutasAsync(int busquedaId);
    Task<List<BusquedasRuta>> ObtenerPorUsuarioAsync(int usuarioId, int limite = 50);
    Task<List<ParesMoneda>> ObtenerParesActivosAsync();
    Task AgregarBusquedaAsync(BusquedasRuta busqueda);
    Task AgregarRutaAsync(RutasConversion ruta);
    Task AgregarSaltoAsync(RutaConversionSaltos salto);
    Task GuardarCambiosAsync();
}
