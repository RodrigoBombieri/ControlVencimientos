using ControlVencimientosP.Domain;

namespace ControlVencimientosP.Services;

/// <summary>
/// Una fila cruda leida de la base para el calendario: lo minimo para poder
/// calcular el semaforo de cada vencimiento y ubicarlo en su dia.
/// </summary>
public record FilaCalendario(
    int VencimientoId,
    int ItemId,
    string ItemNombre,
    DateOnly FechaVencimiento,
    int? DiasAvisoVencimiento,
    int DiasAvisoCategoria);

/// <summary>Un vencimiento ya listo para pintar dentro de un dia de la grilla.</summary>
public record ItemCalendario(int VencimientoId, string Nombre, EstadoSemaforo Estado);

/// <summary>
/// Un dia de la grilla mensual. Puede pertenecer al mes anterior o
/// siguiente: la grilla siempre arranca un lunes y termina un domingo, asi
/// que hace falta relleno de los meses vecinos para completar semanas.
/// </summary>
public record DiaCalendario(
    DateOnly Fecha,
    bool EsDelMesActual,
    bool EsHoy,
    IReadOnlyList<ItemCalendario> Items);

/// <summary>Todo lo que necesita la pantalla de calendario para un mes.</summary>
public class ResumenCalendario
{
    public int Anio { get; init; }
    public int Mes { get; init; }
    public IReadOnlyList<IReadOnlyList<DiaCalendario>> Semanas { get; init; } = [];
}

/// <summary>
/// Arma la grilla mensual del calendario. Funcion pura a proposito, como
/// <see cref="ArmadorDeDashboard"/> y <see cref="ArmadorDeListado"/>: no
/// toca <c>AppDbContext</c>, asi que se testea con una lista de
/// <see cref="FilaCalendario"/> armada a mano, sin base de datos.
/// </summary>
public static class ArmadorDeCalendario
{
    public static ResumenCalendario Armar(IEnumerable<FilaCalendario> filas, int anio, int mes, DateOnly hoy)
    {
        var primerDiaMes = new DateOnly(anio, mes, 1);
        var ultimoDiaMes = primerDiaMes.AddMonths(1).AddDays(-1);

        // La grilla siempre arranca un lunes y termina un domingo, aunque
        // eso implique mostrar dias de los meses vecinos como relleno -
        // asi todas las filas quedan con 7 columnas completas.
        var inicioGrilla = primerDiaMes.AddDays(-DiasDesdeLunes(primerDiaMes.DayOfWeek));
        var finGrilla = ultimoDiaMes.AddDays(DiasHastaDomingo(ultimoDiaMes.DayOfWeek));

        var itemsPorFecha = filas
            .GroupBy(f => f.FechaVencimiento)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<ItemCalendario>)g
                    .Select(f => new ItemCalendario(
                        f.VencimientoId,
                        f.ItemNombre,
                        CalculadoraDeEstado.Calcular(
                            f.FechaVencimiento,
                            CalculadoraDeEstado.DiasAvisoEfectivo(f.DiasAvisoVencimiento, f.DiasAvisoCategoria),
                            hoy)))
                    .OrderBy(i => i.Nombre, StringComparer.OrdinalIgnoreCase)
                    .ToList());

        var dias = new List<DiaCalendario>();
        for (var fecha = inicioGrilla; fecha <= finGrilla; fecha = fecha.AddDays(1))
        {
            itemsPorFecha.TryGetValue(fecha, out var items);
            dias.Add(new DiaCalendario(fecha, fecha.Month == mes, fecha == hoy, items ?? []));
        }

        var semanas = dias
            .Chunk(7)
            .Select(semana => (IReadOnlyList<DiaCalendario>)semana.ToList())
            .ToList();

        return new ResumenCalendario { Anio = anio, Mes = mes, Semanas = semanas };
    }

    // DayOfWeek de .NET: Domingo=0 ... Sabado=6. Esta pantalla arranca la
    // semana en lunes, asi que hay que correr el indice para que Lunes=0.
    private static int DiasDesdeLunes(DayOfWeek dia) => ((int)dia + 6) % 7;

    private static int DiasHastaDomingo(DayOfWeek dia) => (7 - (int)dia) % 7;
}
