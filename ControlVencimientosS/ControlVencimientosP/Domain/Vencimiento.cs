namespace ControlVencimientosP.Domain;

/// <summary>
/// El certificado / poliza / habilitacion concreta, con su fecha.
/// Renovar no es editar la fecha: es crear un Vencimiento nuevo y marcar el
/// anterior como <see cref="EstadoVencimiento.Renovado"/>. Asi el historial
/// sale gratis y se puede auditar cuando se renovo cada cosa.
/// </summary>
public class Vencimiento : IEntidadDeEmpresa
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int ItemId { get; set; }

    public DateOnly? FechaEmision { get; set; }

    /// <summary>
    /// DateOnly a proposito, mapeado a <c>date</c> en SQL Server.
    /// Con <c>datetime</c> aparecen bugs de "vence hoy" segun la hora y el huso.
    /// </summary>
    public DateOnly FechaVencimiento { get; set; }

    public string? NumeroDocumento { get; set; }
    public decimal? Monto { get; set; }
    public string? Moneda { get; set; }

    /// <summary>
    /// Anticipacion propia de este vencimiento. Si es null se usa el
    /// <see cref="Categoria.DiasAvisoDefault"/> de la categoria del item.
    /// </summary>
    public int? DiasAviso { get; set; }

    public EstadoVencimiento Estado { get; set; } = EstadoVencimiento.Activo;

    /// <summary>Apunta al vencimiento que lo reemplazo. Null si es el vigente.</summary>
    public int? RenovadoPorVencimientoId { get; set; }

    public string? CreadoPorUsuarioId { get; set; }
    public DateTime CreadoEn { get; set; }

    public Empresa? Empresa { get; set; }
    public Item? Item { get; set; }
    public Vencimiento? RenovadoPor { get; set; }
    public ICollection<Adjunto> Adjuntos { get; set; } = new List<Adjunto>();
}
