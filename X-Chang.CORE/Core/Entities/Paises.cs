using System;
using System.Collections.Generic;

namespace X_Chang.API.Models;

public partial class Paises
{
    public int PaisId { get; set; }

    public string Nombre { get; set; } = null!;

    public int MonedaId { get; set; }

    public virtual Monedas Moneda { get; set; } = null!;

    public virtual ICollection<Usuarios> Usuarios { get; set; } = new List<Usuarios>();
}
