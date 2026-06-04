using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace X_Chang.API.Models;

public partial class ExchangeDivisasDbContext : DbContext
{
    public ExchangeDivisasDbContext()
    {
    }

    public ExchangeDivisasDbContext(DbContextOptions<ExchangeDivisasDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccesosUsuario> AccesosUsuario { get; set; }

    public virtual DbSet<Billeteras> Billeteras { get; set; }

    public virtual DbSet<CancelacionesOrdenOferta> CancelacionesOrdenOferta { get; set; }

    public virtual DbSet<HistorialTransacciones> HistorialTransacciones { get; set; }

    public virtual DbSet<Monedas> Monedas { get; set; }

    public virtual DbSet<MovimientosBilletera> MovimientosBilletera { get; set; }

    public virtual DbSet<OfertasVenta> OfertasVenta { get; set; }

    public virtual DbSet<OrdenesCompra> OrdenesCompra { get; set; }

    public virtual DbSet<Paises> Paises { get; set; }

    public virtual DbSet<ParesMoneda> ParesMoneda { get; set; }

    public virtual DbSet<Roles> Roles { get; set; }

    public virtual DbSet<SaldosBilletera> SaldosBilletera { get; set; }

    public virtual DbSet<SesionesUsuario> SesionesUsuario { get; set; }

    public virtual DbSet<Usuarios> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer("Server=localhost;Database=ExchangeDivisasDB;Trusted_Connection=True;TrustServerCertificate=True");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccesosUsuario>(entity =>
        {
            entity.HasKey(e => e.AccesoId).HasName("PK__AccesosU__66CA1119D10DEB00");

            entity.HasIndex(e => new { e.UsuarioId, e.FechaAcceso }, "IX_AccesosUsuario_UsuarioFecha").IsDescending(false, true);

            entity.Property(e => e.FechaAcceso).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.MensajeResultado).HasMaxLength(150);
            entity.Property(e => e.MetodoIngreso).HasMaxLength(30);

            entity.HasOne(d => d.Usuario).WithMany(p => p.AccesosUsuario)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AccesosUs__Usuar__2610A626");
        });

        modelBuilder.Entity<Billeteras>(entity =>
        {
            entity.HasKey(e => e.BilleteraId).HasName("PK__Billeter__A3C345531FEBB9DA");

            entity.HasIndex(e => e.UsuarioId, "IX_Billeteras_Usuario");

            entity.HasIndex(e => e.UsuarioId, "UQ__Billeter__2B3DE7B937FF8A3B").IsUnique();

            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Usuario).WithOne(p => p.Billeteras)
                .HasForeignKey<Billeteras>(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Billetera__Usuar__628FA481");
        });

        modelBuilder.Entity<CancelacionesOrdenOferta>(entity =>
        {
            entity.HasKey(e => e.CancelacionId).HasName("PK__Cancelac__5A8447CEF42AA372");

            entity.Property(e => e.CantidadCancelada).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.CantidadEjecutada).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.FechaCancelacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.MontoReembolsado).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.TipoOperacion).HasMaxLength(20);

            entity.HasOne(d => d.OfertaVenta).WithMany(p => p.CancelacionesOrdenOferta)
                .HasForeignKey(d => d.OfertaVentaId)
                .HasConstraintName("FK__Cancelaci__Ofert__14E61A24");

            entity.HasOne(d => d.OrdenCompra).WithMany(p => p.CancelacionesOrdenOferta)
                .HasForeignKey(d => d.OrdenCompraId)
                .HasConstraintName("FK__Cancelaci__Orden__13F1F5EB");

            entity.HasOne(d => d.ParMoneda).WithMany(p => p.CancelacionesOrdenOferta)
                .HasForeignKey(d => d.ParMonedaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cancelaci__ParMo__15DA3E5D");

            entity.HasOne(d => d.Usuario).WithMany(p => p.CancelacionesOrdenOferta)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cancelaci__Usuar__12FDD1B2");
        });

        modelBuilder.Entity<HistorialTransacciones>(entity =>
        {
            entity.HasKey(e => e.HistorialId).HasName("PK__Historia__9752068F8B512AA5");

            entity.HasIndex(e => new { e.UsuarioId, e.FechaHora }, "IX_Historial_UsuarioFecha").IsDescending(false, true);

            entity.Property(e => e.Estado).HasMaxLength(30);
            entity.Property(e => e.FechaHora).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.MetodoEjecucion).HasMaxLength(20);
            entity.Property(e => e.TipoOperacion).HasMaxLength(30);

            entity.HasOne(d => d.Moneda).WithMany(p => p.HistorialTransacciones)
                .HasForeignKey(d => d.MonedaId)
                .HasConstraintName("FK__Historial__Moned__793DFFAF");

            entity.HasOne(d => d.ParMoneda).WithMany(p => p.HistorialTransacciones)
                .HasForeignKey(d => d.ParMonedaId)
                .HasConstraintName("FK__Historial__ParMo__7849DB76");

            entity.HasOne(d => d.Usuario).WithMany(p => p.HistorialTransacciones)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Historial__Usuar__7755B73D");
        });

        modelBuilder.Entity<Monedas>(entity =>
        {
            entity.HasKey(e => e.MonedaId).HasName("PK__Monedas__CEEBACBE0789B7F7");

            entity.HasIndex(e => e.CodigoIso, "UQ__Monedas__F2D697467ECB6370").IsUnique();

            entity.Property(e => e.Activa).HasDefaultValue(true);
            entity.Property(e => e.CodigoIso)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("CodigoISO");
            entity.Property(e => e.Nombre).HasMaxLength(80);
            entity.Property(e => e.Tipo).HasMaxLength(30);
        });

        modelBuilder.Entity<MovimientosBilletera>(entity =>
        {
            entity.HasKey(e => e.MovimientoId).HasName("PK__Movimien__BF923C2C62406BF9");

            entity.Property(e => e.FechaMovimiento).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Monto).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.ReferenciaTipo).HasMaxLength(40);
            entity.Property(e => e.SaldoAnterior).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.SaldoPosterior).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.TipoMovimiento).HasMaxLength(30);

            entity.HasOne(d => d.Moneda).WithMany(p => p.MovimientosBilletera)
                .HasForeignKey(d => d.MonedaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Movimient__Moned__04E4BC85");

            entity.HasOne(d => d.Usuario).WithMany(p => p.MovimientosBilletera)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Movimient__Usuar__03F0984C");
        });

        modelBuilder.Entity<OfertasVenta>(entity =>
        {
            entity.HasKey(e => e.OfertaVentaId).HasName("PK__OfertasV__038B819D08C60B3C");

            entity.HasIndex(e => new { e.ParMonedaId, e.Estado, e.PrecioUnitario }, "IX_OfertasVenta_ParEstadoPrecio");

            entity.HasIndex(e => new { e.UsuarioId, e.Estado, e.FechaCreacion }, "IX_OfertasVenta_UsuarioEstadoFecha").IsDescending(false, false, true);

            entity.Property(e => e.CantidadOriginal).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.CantidadPendiente).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.CantidadVendida).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.Estado)
                .HasMaxLength(30)
                .HasDefaultValue("Activa");
            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.TotalEsperado).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.TotalRecibido).HasColumnType("decimal(28, 8)");

            entity.HasOne(d => d.ParMoneda).WithMany(p => p.OfertasVenta)
                .HasForeignKey(d => d.ParMonedaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OfertasVe__ParMo__3A4CA8FD");

            entity.HasOne(d => d.Usuario).WithMany(p => p.OfertasVenta)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OfertasVe__Usuar__395884C4");
        });

        modelBuilder.Entity<OrdenesCompra>(entity =>
        {
            entity.HasKey(e => e.OrdenCompraId).HasName("PK__OrdenesC__0B556E3670279B8E");

            entity.HasIndex(e => new { e.ParMonedaId, e.Estado, e.PrecioUnitario }, "IX_OrdenesCompra_ParEstadoPrecio").IsDescending(false, false, true);

            entity.HasIndex(e => new { e.UsuarioId, e.Estado, e.FechaCreacion }, "IX_OrdenesCompra_UsuarioEstadoFecha").IsDescending(false, false, true);

            entity.Property(e => e.CantidadObtenida).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.CantidadOriginal).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.CantidadPendiente).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.Estado)
                .HasMaxLength(30)
                .HasDefaultValue("Activa");
            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.TotalComprometido).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.TotalEjecutado).HasColumnType("decimal(28, 8)");

            entity.HasOne(d => d.ParMoneda).WithMany(p => p.OrdenesCompra)
                .HasForeignKey(d => d.ParMonedaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrdenesCo__ParMo__2B0A656D");

            entity.HasOne(d => d.Usuario).WithMany(p => p.OrdenesCompra)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrdenesCo__Usuar__2A164134");
        });

        modelBuilder.Entity<Paises>(entity =>
        {
            entity.HasKey(e => e.PaisId).HasName("PK__Paises__B501E18537AEB018");

            entity.HasIndex(e => e.Nombre, "UQ__Paises__75E3EFCFB97F6EBA").IsUnique();

            entity.Property(e => e.Nombre).HasMaxLength(100);

            entity.HasOne(d => d.Moneda).WithMany(p => p.Paises)
                .HasForeignKey(d => d.MonedaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Paises__MonedaId__534D60F1");
        });

        modelBuilder.Entity<ParesMoneda>(entity =>
        {
            entity.HasKey(e => e.ParMonedaId).HasName("PK__ParesMon__E8663F7AAF75CECF");

            entity.HasIndex(e => e.MonedaDestinoId, "IX_ParesMoneda_Destino");

            entity.HasIndex(e => e.MonedaOrigenId, "IX_ParesMoneda_Origen");

            entity.HasIndex(e => new { e.MonedaOrigenId, e.MonedaDestinoId }, "IX_ParesMoneda_OrigenDestino");

            entity.HasIndex(e => new { e.MonedaOrigenId, e.MonedaDestinoId }, "UQ__ParesMon__19894B790287A61B").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);

            entity.HasOne(d => d.MonedaDestino).WithMany(p => p.ParesMonedaMonedaDestino)
                .HasForeignKey(d => d.MonedaDestinoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ParesMone__Moned__70DDC3D8");

            entity.HasOne(d => d.MonedaOrigen).WithMany(p => p.ParesMonedaMonedaOrigen)
                .HasForeignKey(d => d.MonedaOrigenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ParesMone__Moned__6FE99F9F");
        });

        modelBuilder.Entity<Roles>(entity =>
        {
            entity.HasKey(e => e.RolId).HasName("PK__Roles__F92302F176C09BAB");

            entity.HasIndex(e => e.Nombre, "UQ__Roles__75E3EFCFB879AEE0").IsUnique();

            entity.Property(e => e.Nombre).HasMaxLength(30);
        });

        modelBuilder.Entity<SaldosBilletera>(entity =>
        {
            entity.HasKey(e => e.SaldoId).HasName("PK__SaldosBi__FF916F69E9AA2122");

            entity.HasIndex(e => new { e.BilleteraId, e.SaldoDisponible }, "IX_SaldosBilletera_BilleteraSaldo").IsDescending(false, true);

            entity.HasIndex(e => new { e.BilleteraId, e.MonedaId }, "UQ__SaldosBi__5F2DFF99DF454FF4").IsUnique();

            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.SaldoDisponible).HasColumnType("decimal(28, 8)");

            entity.HasOne(d => d.Billetera).WithMany(p => p.SaldosBilletera)
                .HasForeignKey(d => d.BilleteraId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SaldosBil__Bille__693CA210");

            entity.HasOne(d => d.Moneda).WithMany(p => p.SaldosBilletera)
                .HasForeignKey(d => d.MonedaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SaldosBil__Moned__6A30C649");
        });

        modelBuilder.Entity<SesionesUsuario>(entity =>
        {
            entity.HasKey(e => e.SesionId).HasName("PK__Sesiones__52FD7C665E311931");

            entity.HasIndex(e => new { e.UsuarioId, e.Estado }, "IX_SesionesUsuario_UsuarioEstado");

            entity.HasIndex(e => e.TokenSesion, "UQ__Sesiones__567B1115E3B3DAE8").IsUnique();

            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Activa");
            entity.Property(e => e.FechaInicio).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TokenSesion).HasMaxLength(500);

            entity.HasOne(d => d.Usuario).WithMany(p => p.SesionesUsuario)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SesionesU__Usuar__214BF109");
        });

        modelBuilder.Entity<Usuarios>(entity =>
        {
            entity.HasKey(e => e.UsuarioId).HasName("PK__Usuarios__2B3DE7B810294C22");

            entity.HasIndex(e => e.CorreoElectronico, "IX_Usuarios_Correo");

            entity.HasIndex(e => e.NombreUsuario, "IX_Usuarios_NombreUsuario");

            entity.HasIndex(e => e.CorreoElectronico, "UQ__Usuarios__531402F3606C3BB4").IsUnique();

            entity.HasIndex(e => e.NombreUsuario, "UQ__Usuarios__6B0F5AE0C339C1FA").IsUnique();

            entity.Property(e => e.CorreoElectronico).HasMaxLength(100);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Activo");
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.NombreUsuario).HasMaxLength(30);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.TemaVisual)
                .HasMaxLength(10)
                .HasDefaultValue("Claro");

            entity.HasOne(d => d.Pais).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.PaisId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuarios__PaisId__5DCAEF64");

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuarios__RolId__5CD6CB2B");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
