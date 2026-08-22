namespace ControlVencimientosP.Services;

/// <summary>
/// Una fila cruda leida de la base para el listado completo: como
/// <see cref="FilaVencimiento"/> del dashboard, pero con los datos extra
/// que hacen falta para filtrar y mostrar la tabla completa (ubicación del
/// item, id de categoría).
/// </summary>
public record FilaListado(
    int VencimientoId,
    int ItemId,
    string ItemNombre,
    string? ItemCodigo,
    string? ItemUbicacion,
    int CategoriaId,
    string CategoriaNombre,
    string CategoriaIcono,
    DateOnly FechaVencimiento,
    int? DiasAvisoVencimiento,
    int DiasAvisoCategoria);

/// <summary>Una fila ya lista para pintar en la tabla del listado.</summary>
public record FilaVencimientoListado(
    int VencimientoId,
    int ItemId,
    string Nombre,
    string? Codigo,
    string? Ubicacion,
    int CategoriaId,
    string CategoriaNombre,
    string CategoriaIcono,
    DateOnly FechaVencimiento,
    int DiasRestantes,
    EstadoSemaforo Estado);

/// <summary>
/// Calcula el estado de cada fila y aplica los filtros de la pantalla de
/// listado. Dos pasos separados (Calcular / Filtrar) a propósito: el
/// controller calcula una sola vez y puede filtrar más de una vez sobre el
/// mismo resultado (por ejemplo, para sacar los totales sin filtrar y la
/// lista filtrada en la misma request) sin repetir el cálculo del semáforo.
/// </summary>
public static class ArmadorDeListado
{
    public static IReadOnlyList<FilaVencimientoListado> Calcular(
        IEnumerable<FilaListado> filas, DateOnly hoy)
        => filas
            .Select(f =>
            {
                var diasAviso = CalculadoraDeEstado.DiasAvisoEfectivo(f.DiasAvisoVencimiento, f.DiasAvisoCategoria);
                var estado = CalculadoraDeEstado.Calcular(f.FechaVencimiento, diasAviso, hoy);
                var diasRestantes = CalculadoraDeEstado.DiasRestantes(f.FechaVencimiento, hoy);

                return new FilaVencimientoListado(
                    f.VencimientoId, f.ItemId, f.ItemNombre, f.ItemCodigo, f.ItemUbicacion,
                    f.CategoriaId, f.CategoriaNombre, f.CategoriaIcono,
                    f.FechaVencimiento, diasRestantes, estado);
            })
            .OrderBy(i => i.FechaVencimiento)
            .ToList();

    public static IReadOnlyList<FilaVencimientoListado> Filtrar(
        IEnumerable<FilaVencimientoListado> items,
        EstadoSemaforo? estado = null,
        int? categoriaId = null,
        string? texto = null)
    {
        var resultado = items;

        if (estado.HasValue)
            resultado = resultado.Where(i => i.Estado == estado.Value);

        if (categoriaId.HasValue)
            resultado = resultado.Where(i => i.CategoriaId == categoriaId.Value);

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var t = texto.Trim();
            resultado = resultado.Where(i =>
                i.Nombre.Contains(t, StringComparison.OrdinalIgnoreCase) ||
                (i.Codigo?.Contains(t, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (i.Ubicacion?.Contains(t, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return resultado.ToList();
    }
}
