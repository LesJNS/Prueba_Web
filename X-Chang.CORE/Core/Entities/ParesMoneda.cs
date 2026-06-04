using System;
using System.Collections.Generic;

namespace X_Chang.API.Models;

public partial class ParesMoneda
{
    public int ParMonedaId { get; set; }

    public int MonedaOrigenId { get; set; }

    public int MonedaDestinoId { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<CancelacionesOrdenOferta> CancelacionesOrdenOferta { get; set; } = new List<CancelacionesOrdenOferta>();

    public virtual ICollection<HistorialTransacciones> HistorialTransacciones { get; set; } = new List<HistorialTransacciones>();

    public virtual Monedas MonedaDestino { get; set; } = null!;

    public virtual Monedas MonedaOrigen { get; set; } = null!;

    public virtual ICollection<OfertasVenta> OfertasVenta { get; set; } = new List<OfertasVenta>();

    public virtual ICollection<OrdenesCompra> OrdenesCompra { get; set; } = new List<OrdenesCompra>();
}
