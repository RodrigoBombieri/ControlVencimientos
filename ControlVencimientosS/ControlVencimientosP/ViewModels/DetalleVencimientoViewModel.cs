namespace ControlVencimientosP.ViewModels;

/// <summary>Todo lo que necesita la pantalla de detalle de un vencimiento.</summary>
public class DetalleVencimientoViewModel
{
    public int VencimientoId { get; init; }
    public int ItemId { get; init; }
    public string ItemNombre { get; init; } = string.Empty;
    public string? ItemCodigo { get; init; }
    public string? ItemUbicacion { get; init; }
    public string? ItemProveedor { get; init; }
    public string CategoriaNombre { get; init; } = string.Empty;

    public DateOnly? FechaEmision { get; init; }
    public DateOnly FechaVencimiento { get; init; }
    public string? NumeroDocumento { get; init; }
    public decimal? Monto { get; init; }
    public string? Moneda { get; init; }

    public EstadoVencimiento Estado { get; init; }
    public EstadoSemaforo EstadoSemaforo { get; init; }
    public int DiasRestantes { get; init; }
    public DateTime CreadoEn { get; init; }

    public IReadOnlyList<Adjunto> Adjuntos { get; init; } = [];
}
