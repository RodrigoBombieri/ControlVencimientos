using ControlVencimientosP.Domain;

namespace ControlVencimientosP.Services;

/// <summary>
/// Filtros que se traducen a SQL y usan el indice
/// (EmpresaId, Estado, FechaVencimiento).
/// El semaforo se calcula en memoria; lo que va contra la base es la fecha.
/// </summary>
public static class ConsultasDeVencimiento
{
    /// <summary>Solo el vencimiento vigente de cada item, sin el historial.</summary>
    public static IQueryable<Vencimiento> Activos(this IQueryable<Vencimiento> consulta)
        => consulta.Where(v => v.Estado == EstadoVencimiento.Activo);

    public static IQueryable<Vencimiento> Vencidos(this IQueryable<Vencimiento> consulta, DateOnly hoy)
        => consulta.Activos().Where(v => v.FechaVencimiento < hoy);

    /// <summary>Rango inclusivo: lo que vence entre hoy y <paramref name="limite"/>.</summary>
    public static IQueryable<Vencimiento> VencenHasta(
        this IQueryable<Vencimiento> consulta, DateOnly hoy, DateOnly limite)
        => consulta.Activos().Where(v => v.FechaVencimiento >= hoy && v.FechaVencimiento <= limite);
}
