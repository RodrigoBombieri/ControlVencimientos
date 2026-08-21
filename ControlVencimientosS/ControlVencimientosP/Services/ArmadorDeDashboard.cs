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
    int ItemId,
    string Nombre,
    string? Codigo,
    string CategoriaNombre,
    string CategoriaIcono,
    DateOnly FechaVencimiento,
    int DiasRestantes,
    EstadoSemaforo Estado);

/// <summary>Todo lo que necesita la pantalla de inicio.</summary>
public class ResumenDashboard
{
    public DateOnly Hoy { get; init; }
    public int TotalActivos { get; init; }
    public int Vigentes { get; init; }
    public int PorVencer { get; init; }
    public int Vencidos { get; init; }
    public IReadOnlyList<ItemDashboard> Proximos { get; init; } = [];
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
        IEnumerable<FilaVencimiento> filas, DateOnly hoy, int maxProximos = 8)
    {
        var items = filas
            .Select(f =>
            {
                var diasAviso = CalculadoraDeEstado.DiasAvisoEfectivo(f.DiasAvisoVencimiento, f.DiasAvisoCategoria);
                var estado = CalculadoraDeEstado.Calcular(f.FechaVencimiento, diasAviso, hoy);
                var diasRestantes = CalculadoraDeEstado.DiasRestantes(f.FechaVencimiento, hoy);

                return new ItemDashboard(
                    f.ItemId, f.ItemNombre, f.ItemCodigo, f.CategoriaNombre, f.CategoriaIcono,
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
                .ToList()
        };
    }
}
