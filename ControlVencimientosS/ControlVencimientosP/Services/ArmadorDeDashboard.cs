using ControlVencimientosP.Domain;

namespace ControlVencimientosP.Services;

/// <summary>
/// Una fila cruda leida de la base: un vencimiento activo con los datos de su
/// item y su categoria ya "aplanados". Es lo minimo que necesita
/// <see cref="ArmadorDeDashboard"/> para calcular el resto.
/// </summary>
public record FilaVencimiento(
    int VencimientoId,
    int ItemId,
    string ItemNombre,
    string? ItemCodigo,
    string CategoriaNombre,
    string CategoriaIcono,
    DateOnly FechaVencimiento,
    int? DiasAvisoVencimiento,
    int DiasAvisoCategoria);

/// <summary>Una fila ya lista para pintar en la tabla de "proximos vencimientos".</summary>
public record ItemDashboard(
    int VencimientoId,
    int ItemId,
    string Nombre,
    string? Codigo,
    string CategoriaNombre,
    string CategoriaIcono,
    DateOnly FechaVencimiento,
    int DiasRestantes,
    EstadoSemaforo Estado);

/// <summary>
/// Cuantos vencimientos activos caen en un mes calendario. Cuenta por fecha,
/// no por semaforo: un vencimiento que ya paso pero es de este mes suma igual,
/// asi el grafico muestra el volumen real del mes, no solo lo pendiente.
/// </summary>
public record MesDashboard(int Anio, int Mes, int Cantidad);

/// <summary>Todo lo que necesita la pantalla de inicio.</summary>
public class ResumenDashboard
{
    public DateOnly Hoy { get; init; }
    public int TotalActivos { get; init; }
    public int Vigentes { get; init; }
    public int PorVencer { get; init; }
    public int Vencidos { get; init; }
    public IReadOnlyList<ItemDashboard> Proximos { get; init; } = [];
    public IReadOnlyList<MesDashboard> PorMes { get; init; } = [];

    /// <summary>
    /// Se completa aparte, despues de <see cref="ArmadorDeDashboard.Armar"/>:
    /// depende de la configuracion de avisos y de los destinatarios, algo que
    /// no tiene nada que ver con "armar el resumen de vencimientos". Por eso
    /// no es <c>init</c> como el resto de la clase: el controller la asigna
    /// una vez que la calcula con <see cref="ArmadorDeAvisos"/>.
    /// </summary>
    public ProximoAviso? ProximoAviso { get; set; }
}

/// <summary>
/// Convierte filas crudas de la base en el resumen del dashboard. Es una
/// funcion pura a proposito: no toca <c>AppDbContext</c>, asi que se puede
/// testear con una lista de <see cref="FilaVencimiento"/> armada a mano, sin
/// levantar una base de datos.
/// </summary>
public static class ArmadorDeDashboard
{
    public static ResumenDashboard Armar(
        IEnumerable<FilaVencimiento> filas, DateOnly hoy, int maxProximos = 8, int mesesGrafico = 6)
    {
        var items = filas
            .Select(f =>
            {
                var diasAviso = CalculadoraDeEstado.DiasAvisoEfectivo(f.DiasAvisoVencimiento, f.DiasAvisoCategoria);
                var estado = CalculadoraDeEstado.Calcular(f.FechaVencimiento, diasAviso, hoy);
                var diasRestantes = CalculadoraDeEstado.DiasRestantes(f.FechaVencimiento, hoy);

                return new ItemDashboard(
                    f.VencimientoId, f.ItemId, f.ItemNombre, f.ItemCodigo, f.CategoriaNombre, f.CategoriaIcono,
                    f.FechaVencimiento, diasRestantes, estado);
            })
            .ToList();

        return new ResumenDashboard
        {
            Hoy = hoy,
            TotalActivos = items.Count,
            Vigentes = items.Count(i => i.Estado == EstadoSemaforo.Vigente),
            PorVencer = items.Count(i => i.Estado == EstadoSemaforo.PorVencer),
            Vencidos = items.Count(i => i.Estado == EstadoSemaforo.Vencido),
            // Orden por fecha ascendente: alcanza con un solo criterio porque
            // "vencido" es simplemente una fecha en el pasado. Lo mas atrasado
            // queda arriba de todo, despues lo que esta por vencer, en el
            // mismo orden que pide el mockup.
            Proximos = items
                .OrderBy(i => i.FechaVencimiento)
                .Take(maxProximos)
                .ToList(),
            PorMes = ArmarPorMes(items, hoy, mesesGrafico)
        };
    }

    /// <summary>
    /// Un balde por mes, empezando en el mes de <paramref name="hoy"/>. Los
    /// meses sin ningun vencimiento igual aparecen, en cero: el grafico
    /// siempre muestra los mismos N meses, nunca "salta" uno vacio.
    /// </summary>
    private static IReadOnlyList<MesDashboard> ArmarPorMes(
        IReadOnlyList<ItemDashboard> items, DateOnly hoy, int meses)
    {
        var baldes = new List<MesDashboard>(meses);

        for (var i = 0; i < meses; i++)
        {
            var fecha = hoy.AddMonths(i);
            var cantidad = items.Count(it =>
                it.FechaVencimiento.Year == fecha.Year && it.FechaVencimiento.Month == fecha.Month);

            baldes.Add(new MesDashboard(fecha.Year, fecha.Month, cantidad));
        }

        return baldes;
    }
}
