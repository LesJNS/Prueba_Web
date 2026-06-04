using System;
using System.Collections.Generic;

namespace X_Chang.API.Models;

public partial class Usuarios
{
    public int UsuarioId { get; set; }

    public int RolId { get; set; }

    public int PaisId { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string CorreoElectronico { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string TemaVisual { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }

    public DateTime? FechaUltimoAcceso { get; set; }

    public virtual ICollection<AccesosUsuario> AccesosUsuario { get; set; } = new List<AccesosUsuario>();

    public virtual Billeteras? Billeteras { get; set; }

    public virtual ICollection<CancelacionesOrdenOferta> CancelacionesOrdenOferta { get; set; } = new List<CancelacionesOrdenOferta>();

    public virtual ICollection<HistorialTransacciones> HistorialTransacciones { get; set; } = new List<HistorialTransacciones>();

    public virtual ICollection<MovimientosBilletera> MovimientosBilletera { get; set; } = new List<MovimientosBilletera>();

    public virtual ICollection<OfertasVenta> OfertasVenta { get; set; } = new List<OfertasVenta>();

    public virtual ICollection<OrdenesCompra> OrdenesCompra { get; set; } = new List<OrdenesCompra>();

    public virtual Paises Pais { get; set; } = null!;

    public virtual Roles Rol { get; set; } = null!;

    public virtual ICollection<SesionesUsuario> SesionesUsuario { get; set; } = new List<SesionesUsuario>();
}
