using System;
using System.Collections.Generic;

namespace X_Chang.API.Models;

public partial class OfertasVenta
{
    public int OfertaVentaId { get; set; }

    public int UsuarioId { get; set; }

    public int ParMonedaId { get; set; }

    public decimal CantidadOriginal { get; set; }

    public decimal CantidadVendida { get; set; }

    public decimal CantidadPendiente { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal TotalEsperado { get; set; }

    public decimal TotalRecibido { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public DateTime? FechaCancelacion { get; set; }

    public virtual ICollection<CancelacionesOrdenOferta> CancelacionesOrdenOferta { get; set; } = new List<CancelacionesOrdenOferta>();

    public virtual ParesMoneda ParMoneda { get; set; } = null!;

    public virtual Usuarios Usuario { get; set; } = null!;
}
