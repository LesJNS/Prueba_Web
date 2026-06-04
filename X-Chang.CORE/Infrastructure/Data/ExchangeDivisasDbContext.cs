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

    public virtual DbSet<AdjuntosCorreo> AdjuntosCorreo { get; set; }

    public virtual DbSet<AuditoriaAdministrativa> AuditoriaAdministrativa { get; set; }

    public virtual DbSet<Billeteras> Billeteras { get; set; }

    public virtual DbSet<BusquedasRuta> BusquedasRuta { get; set; }

    public virtual DbSet<CancelacionesOrdenOferta> CancelacionesOrdenOferta { get; set; }

    public virtual DbSet<ConfiguracionSistema> ConfiguracionSistema { get; set; }

    public virtual DbSet<Depositos> Depositos { get; set; }

    public virtual DbSet<EjecucionesOrden> EjecucionesOrden { get; set; }

    public virtual DbSet<ExportacionesReporte> ExportacionesReporte { get; set; }

    public virtual DbSet<HistorialTransacciones> HistorialTransacciones { get; set; }

    public virtual DbSet<HistoricoPreciosPar> HistoricoPreciosPar { get; set; }

    public virtual DbSet<MetodosPago> MetodosPago { get; set; }

    public virtual DbSet<MetodosPagoPais> MetodosPagoPais { get; set; }

    public virtual DbSet<Monedas> Monedas { get; set; }

    public virtual DbSet<MovimientosBilletera> MovimientosBilletera { get; set; }

    public virtual DbSet<NotificacionesCorreo> NotificacionesCorreo { get; set; }

    public virtual DbSet<OfertasVenta> OfertasVenta { get; set; }

    public virtual DbSet<OperacionInmediataEjecuciones> OperacionInmediataEjecuciones { get; set; }

    public virtual DbSet<OperacionesInmediatas> OperacionesInmediatas { get; set; }

    public virtual DbSet<OrdenesCompra> OrdenesCompra { get; set; }

    public virtual DbSet<Paises> Paises { get; set; }

    public virtual DbSet<ParesMoneda> ParesMoneda { get; set; }

    public virtual DbSet<RestriccionesUsuario> RestriccionesUsuario { get; set; }

    public virtual DbSet<Retiros> Retiros { get; set; }

    public virtual DbSet<Roles> Roles { get; set; }

    public virtual DbSet<RutaConversionSaltos> RutaConversionSaltos { get; set; }

    public virtual DbSet<RutasConversion> RutasConversion { get; set; }

    public virtual DbSet<SaldosBilletera> SaldosBilletera { get; set; }

    public virtual DbSet<SesionesUsuario> SesionesUsuario { get; set; }

    public virtual DbSet<TiposNotificacion> TiposNotificacion { get; set; }

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

        modelBuilder.Entity<AdjuntosCorreo>(entity =>
        {
            entity.HasKey(e => e.AdjuntoId).HasName("PK__Adjuntos__2ECBD5404D4D7E51");

            entity.Property(e => e.NombreArchivo).HasMaxLength(255);
            entity.Property(e => e.TipoContenido).HasMaxLength(100);
            entity.Property(e => e.UrlArchivo).HasMaxLength(500);

            entity.HasOne(d => d.Notificacion).WithMany(p => p.AdjuntosCorreo)
                .HasForeignKey(d => d.NotificacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AdjuntosC__Notif__01D345B0");
        });

        modelBuilder.Entity<AuditoriaAdministrativa>(entity =>
        {
            entity.HasKey(e => e.AuditoriaId).HasName("PK__Auditori__095694C3BBA90FE2");

            entity.HasIndex(e => new { e.AdministradorId, e.FechaHora }, "IX_Auditoria_AdministradorFecha").IsDescending(false, true);

            entity.HasIndex(e => e.FechaHora, "IX_Auditoria_Fecha").IsDescending();

            entity.HasIndex(e => new { e.UsuarioAfectadoId, e.FechaHora }, "IX_Auditoria_UsuarioAfectadoFecha").IsDescending(false, true);

            entity.Property(e => e.FechaHora).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.MensajeRegistrado).HasMaxLength(300);
            entity.Property(e => e.TipoAccion).HasMaxLength(30);

            entity.HasOne(d => d.Administrador).WithMany(p => p.AuditoriaAdministrativaAdministrador)
                .HasForeignKey(d => d.AdministradorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Auditoria__Admin__0697FACD");

            entity.HasOne(d => d.UsuarioAfectado).WithMany(p => p.AuditoriaAdministrativaUsuarioAfectado)
                .HasForeignKey(d => d.UsuarioAfectadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Auditoria__Usuar__078C1F06");
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

        modelBuilder.Entity<BusquedasRuta>(entity =>
        {
            entity.HasKey(e => e.BusquedaRutaId).HasName("PK__Busqueda__42CC8A1BB681BF4D");

            entity.HasIndex(e => new { e.UsuarioId, e.Estado }, "IX_BusquedasRuta_UsuarioEstado");

            entity.Property(e => e.AhorroEstimado).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.CantidadSolicitada).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Pendiente");
            entity.Property(e => e.FechaInicio).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.GananciaEstimada).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.TipoOperacion).HasMaxLength(20);
            entity.Property(e => e.TotalNormal).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.TotalRuta).HasColumnType("decimal(28, 8)");

            entity.HasOne(d => d.ParMoneda).WithMany(p => p.BusquedasRuta)
                .HasForeignKey(d => d.ParMonedaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Busquedas__ParMo__5F7E2DAC");

            entity.HasOne(d => d.Usuario).WithMany(p => p.BusquedasRuta)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Busquedas__Usuar__5E8A0973");
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

        modelBuilder.Entity<ConfiguracionSistema>(entity =>
        {
            entity.HasKey(e => e.ConfiguracionId).HasName("PK__Configur__9B95E036B1370CB1");

            entity.HasIndex(e => e.Clave, "UQ__Configur__E8181E11435275E9").IsUnique();

            entity.Property(e => e.Clave).HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Valor).HasMaxLength(300);
        });

        modelBuilder.Entity<Depositos>(entity =>
        {
            entity.HasKey(e => e.DepositoId).HasName("PK__Deposito__345C2198DAB61929");

            entity.HasIndex(e => new { e.UsuarioId, e.FechaDeposito }, "IX_Depositos_UsuarioFecha").IsDescending(false, true);

            entity.Property(e => e.ComisionAplicada).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Completada");
            entity.Property(e => e.FechaDeposito).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.MontoDepositado).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.TotalPagado).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.VoucherUrl).HasMaxLength(500);

            entity.HasOne(d => d.MetodoPago).WithMany(p => p.Depositos)
                .HasForeignKey(d => d.MetodoPagoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Depositos__Metod__10566F31");

            entity.HasOne(d => d.Moneda).WithMany(p => p.Depositos)
                .HasForeignKey(d => d.MonedaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Depositos__Moned__0F624AF8");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Depositos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Depositos__Usuar__0E6E26BF");
        });

        modelBuilder.Entity<EjecucionesOrden>(entity =>
        {
            entity.HasKey(e => e.EjecucionId).HasName("PK__Ejecucio__4C9F90755722F7BC");

            entity.HasIndex(e => e.FechaEjecucion, "IX_EjecucionesOrden_Fecha");

            entity.Property(e => e.CantidadEjecutada).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.FechaEjecucion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.TotalOperacion).HasColumnType("decimal(28, 8)");

            entity.HasOne(d => d.Comprador).WithMany(p => p.EjecucionesOrdenComprador)
                .HasForeignKey(d => d.CompradorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ejecucion__Compr__43D61337");

            entity.HasOne(d => d.OfertaVenta).WithMany(p => p.EjecucionesOrden)
                .HasForeignKey(d => d.OfertaVentaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ejecucion__Ofert__41EDCAC5");

            entity.HasOne(d => d.OrdenCompra).WithMany(p => p.EjecucionesOrden)
                .HasForeignKey(d => d.OrdenCompraId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ejecucion__Orden__40F9A68C");

            entity.HasOne(d => d.ParMoneda).WithMany(p => p.EjecucionesOrden)
                .HasForeignKey(d => d.ParMonedaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ejecucion__ParMo__42E1EEFE");

            entity.HasOne(d => d.Vendedor).WithMany(p => p.EjecucionesOrdenVendedor)
                .HasForeignKey(d => d.VendedorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ejecucion__Vende__44CA3770");
        });

        modelBuilder.Entity<ExportacionesReporte>(entity =>
        {
            entity.HasKey(e => e.ExportacionId).HasName("PK__Exportac__85D26A5F581367FC");

            entity.HasIndex(e => new { e.UsuarioId, e.FechaExportacion }, "IX_ExportacionesReporte_UsuarioFecha").IsDescending(false, true);

            entity.Property(e => e.FechaExportacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Formato).HasMaxLength(10);
            entity.Property(e => e.TipoReporte).HasMaxLength(40);
            entity.Property(e => e.UrlArchivo).HasMaxLength(500);

            entity.HasOne(d => d.Usuario).WithMany(p => p.ExportacionesReporte)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Exportaci__Usuar__318258D2");
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

        modelBuilder.Entity<HistoricoPreciosPar>(entity =>
        {
            entity.HasKey(e => e.HistoricoPrecioId).HasName("PK__Historic__1F78ECE4B01FEA34");

            entity.HasIndex(e => new { e.ParMonedaId, e.FechaRegistro }, "IX_HistoricoPreciosPar_ParFecha").IsDescending(false, true);

            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Margen)
                .HasComputedColumnSql("(case when [MayorPrecioCompra] IS NOT NULL AND [MenorPrecioVenta] IS NOT NULL then [MenorPrecioVenta]-[MayorPrecioCompra]  end)", false)
                .HasColumnType("decimal(29, 8)");
            entity.Property(e => e.MayorPrecioCompra).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.MenorPrecioVenta).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.VolumenCompra).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.VolumenVenta).HasColumnType("decimal(28, 8)");

            entity.HasOne(d => d.ParMoneda).WithMany(p => p.HistoricoPreciosPar)
                .HasForeignKey(d => d.ParMonedaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Historico__ParMo__0D44F85C");
        });

        modelBuilder.Entity<MetodosPago>(entity =>
        {
            entity.HasKey(e => e.MetodoPagoId).HasName("PK__MetodosP__A8FEAF543AA7C709");

            entity.HasIndex(e => e.Nombre, "UQ__MetodosP__75E3EFCFC02DE673").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.ComisionFija).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.ComisionPorcentaje).HasColumnType("decimal(10, 6)");
            entity.Property(e => e.Nombre).HasMaxLength(50);
            entity.Property(e => e.Tipo).HasMaxLength(20);
        });

        modelBuilder.Entity<MetodosPagoPais>(entity =>
        {
            entity.HasKey(e => e.MetodoPagoPaisId).HasName("PK__MetodosP__A0592B96F9692133");

            entity.HasIndex(e => new { e.PaisId, e.Activo }, "IX_MetodosPagoPais_PaisActivo");

            entity.HasIndex(e => new { e.MetodoPagoId, e.PaisId }, "UQ__MetodosP__E3AEB14DE6E3F85E").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);

            entity.HasOne(d => d.MetodoPago).WithMany(p => p.MetodosPagoPais)
                .HasForeignKey(d => d.MetodoPagoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MetodosPa__Metod__7E37BEF6");

            entity.HasOne(d => d.Pais).WithMany(p => p.MetodosPagoPais)
                .HasForeignKey(d => d.PaisId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MetodosPa__PaisI__7F2BE32F");
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

        modelBuilder.Entity<NotificacionesCorreo>(entity =>
        {
            entity.HasKey(e => e.NotificacionId).HasName("PK__Notifica__BCC1202407EF8A8C");

            entity.HasIndex(e => new { e.EstadoEnvio, e.FechaCreacion }, "IX_NotificacionesCorreo_EstadoFecha");

            entity.Property(e => e.Asunto).HasMaxLength(150);
            entity.Property(e => e.CorreoDestino).HasMaxLength(100);
            entity.Property(e => e.EstadoEnvio)
                .HasMaxLength(20)
                .HasDefaultValue("Pendiente");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.ReferenciaTipo).HasMaxLength(40);
            entity.Property(e => e.TipoEvento).HasMaxLength(60);

            entity.HasOne(d => d.TipoNotificacion).WithMany(p => p.NotificacionesCorreo)
                .HasForeignKey(d => d.TipoNotificacionId)
                .HasConstraintName("FK_NotificacionesCorreo_TiposNotificacion");

            entity.HasOne(d => d.Usuario).WithMany(p => p.NotificacionesCorreo)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificac__Usuar__7EF6D905");
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

        modelBuilder.Entity<OperacionInmediataEjecuciones>(entity =>
        {
            entity.HasKey(e => e.OperacionInmediataEjecucionId).HasName("PK__Operacio__A40EB128DA13F5BA");

            entity.HasOne(d => d.Ejecucion).WithMany(p => p.OperacionInmediataEjecuciones)
                .HasForeignKey(d => d.EjecucionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Operacion__Ejecu__540C7B00");

            entity.HasOne(d => d.OperacionInmediata).WithMany(p => p.OperacionInmediataEjecuciones)
                .HasForeignKey(d => d.OperacionInmediataId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Operacion__Opera__531856C7");
        });

        modelBuilder.Entity<OperacionesInmediatas>(entity =>
        {
            entity.HasKey(e => e.OperacionInmediataId).HasName("PK__Operacio__D7DB43E41DBF16CD");

            entity.HasIndex(e => new { e.UsuarioId, e.FechaOperacion }, "IX_OperacionesInmediatas_UsuarioFecha").IsDescending(false, true);

            entity.Property(e => e.CantidadEjecutada).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.CantidadSolicitada).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Completada");
            entity.Property(e => e.FechaOperacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.MetodoEjecucion).HasMaxLength(20);
            entity.Property(e => e.PrecioMaximo).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.PrecioMinimo).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.PrecioPromedio).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.TipoOperacion).HasMaxLength(20);
            entity.Property(e => e.TotalPagado).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.TotalRecibido).HasColumnType("decimal(28, 8)");

            entity.HasOne(d => d.OperacionPadre).WithMany(p => p.InverseOperacionPadre)
                .HasForeignKey(d => d.OperacionPadreId)
                .HasConstraintName("FK_OperacionesInmediatas_OperacionPadre");

            entity.HasOne(d => d.ParMoneda).WithMany(p => p.OperacionesInmediatas)
                .HasForeignKey(d => d.ParMonedaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Operacion__ParMo__503BEA1C");

            entity.HasOne(d => d.Usuario).WithMany(p => p.OperacionesInmediatas)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Operacion__Usuar__4F47C5E3");
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

        modelBuilder.Entity<RestriccionesUsuario>(entity =>
        {
            entity.HasKey(e => e.RestriccionId).HasName("PK__Restricc__26D13DE6ABA547EB");

            entity.HasIndex(e => new { e.UsuarioId, e.EstadoRestriccion }, "IX_RestriccionesUsuario_UsuarioEstado");

            entity.Property(e => e.EstadoRestriccion)
                .HasMaxLength(20)
                .HasDefaultValue("Activa");
            entity.Property(e => e.FechaInicio).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Mensaje).HasMaxLength(300);
            entity.Property(e => e.TipoAccion).HasMaxLength(20);

            entity.HasOne(d => d.Administrador).WithMany(p => p.RestriccionesUsuarioAdministrador)
                .HasForeignKey(d => d.AdministradorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Restricci__Admin__1A9EF37A");

            entity.HasOne(d => d.Usuario).WithMany(p => p.RestriccionesUsuarioUsuario)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Restricci__Usuar__19AACF41");
        });

        modelBuilder.Entity<Retiros>(entity =>
        {
            entity.HasKey(e => e.RetiroId).HasName("PK__Retiros__992834D8D5B51F80");

            entity.HasIndex(e => new { e.UsuarioId, e.FechaRetiro }, "IX_Retiros_UsuarioFecha").IsDescending(false, true);

            entity.Property(e => e.ComisionAplicada).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Completada");
            entity.Property(e => e.FechaRetiro).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.MontoFinalRecibido).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.MontoRetirado).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.VoucherUrl).HasMaxLength(500);

            entity.HasOne(d => d.MetodoPago).WithMany(p => p.Retiros)
                .HasForeignKey(d => d.MetodoPagoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Retiros__MetodoP__1BC821DD");

            entity.HasOne(d => d.Moneda).WithMany(p => p.Retiros)
                .HasForeignKey(d => d.MonedaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Retiros__MonedaI__1AD3FDA4");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Retiros)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Retiros__Usuario__19DFD96B");
        });

        modelBuilder.Entity<Roles>(entity =>
        {
            entity.HasKey(e => e.RolId).HasName("PK__Roles__F92302F176C09BAB");

            entity.HasIndex(e => e.Nombre, "UQ__Roles__75E3EFCFB879AEE0").IsUnique();

            entity.Property(e => e.Nombre).HasMaxLength(30);
        });

        modelBuilder.Entity<RutaConversionSaltos>(entity =>
        {
            entity.HasKey(e => e.RutaConversionSaltoId).HasName("PK__RutaConv__E4D44EF3896B8299");

            entity.HasIndex(e => new { e.RutaConversionId, e.NumeroSalto }, "IX_RutaConversionSaltos_Ruta");

            entity.HasIndex(e => new { e.RutaConversionId, e.NumeroSalto }, "UQ__RutaConv__A841EEA92513F170").IsUnique();

            entity.Property(e => e.CantidadConvertida).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.PrecioMaximo).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.PrecioMinimo).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.PrecioPromedio).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.ResultadoObtenido).HasColumnType("decimal(28, 8)");

            entity.HasOne(d => d.MonedaDestino).WithMany(p => p.RutaConversionSaltosMonedaDestino)
                .HasForeignKey(d => d.MonedaDestinoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RutaConve__Moned__719CDDE7");

            entity.HasOne(d => d.MonedaOrigen).WithMany(p => p.RutaConversionSaltosMonedaOrigen)
                .HasForeignKey(d => d.MonedaOrigenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RutaConve__Moned__70A8B9AE");

            entity.HasOne(d => d.OperacionInmediataHija).WithMany(p => p.RutaConversionSaltosOperacionInmediataHija)
                .HasForeignKey(d => d.OperacionInmediataHijaId)
                .HasConstraintName("FK_RutaConversionSaltos_OperacionHija");

            entity.HasOne(d => d.OperacionInmediata).WithMany(p => p.RutaConversionSaltosOperacionInmediata)
                .HasForeignKey(d => d.OperacionInmediataId)
                .HasConstraintName("FK__RutaConve__Opera__72910220");

            entity.HasOne(d => d.ParMoneda).WithMany(p => p.RutaConversionSaltos)
                .HasForeignKey(d => d.ParMonedaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RutaConve__ParMo__6FB49575");

            entity.HasOne(d => d.RutaConversion).WithMany(p => p.RutaConversionSaltos)
                .HasForeignKey(d => d.RutaConversionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RutaConve__RutaC__6EC0713C");
        });

        modelBuilder.Entity<RutasConversion>(entity =>
        {
            entity.HasKey(e => e.RutaConversionId).HasName("PK__RutasCon__E95E383970476D66");

            entity.Property(e => e.AhorroEstimado).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.GananciaEstimada).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.TotalEstimado).HasColumnType("decimal(28, 8)");

            entity.HasOne(d => d.BusquedaRuta).WithMany(p => p.RutasConversion)
                .HasForeignKey(d => d.BusquedaRutaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RutasConv__Busqu__65370702");

            entity.HasOne(d => d.MonedaFinal).WithMany(p => p.RutasConversionMonedaFinal)
                .HasForeignKey(d => d.MonedaFinalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RutasConv__Moned__681373AD");

            entity.HasOne(d => d.MonedaInicial).WithMany(p => p.RutasConversionMonedaInicial)
                .HasForeignKey(d => d.MonedaInicialId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RutasConv__Moned__671F4F74");

            entity.HasOne(d => d.OperacionInmediata).WithMany(p => p.RutasConversion)
                .HasForeignKey(d => d.OperacionInmediataId)
                .HasConstraintName("FK__RutasConv__Opera__662B2B3B");
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

        modelBuilder.Entity<TiposNotificacion>(entity =>
        {
            entity.HasKey(e => e.TipoNotificacionId).HasName("PK__TiposNot__BE05838D63AA113B");

            entity.HasIndex(e => e.Nombre, "UQ__TiposNot__75E3EFCFCFEE138F").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Descripcion).HasMaxLength(300);
            entity.Property(e => e.Nombre).HasMaxLength(80);
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
