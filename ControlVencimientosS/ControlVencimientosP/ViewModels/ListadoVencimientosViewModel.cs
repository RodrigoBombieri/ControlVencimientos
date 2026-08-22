using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControlVencimientosP.ViewModels;

/// <summary>Todo lo que necesita la pantalla de listado de vencimientos.</summary>
public class ListadoVencimientosViewModel
{
    public IReadOnlyList<FilaVencimientoListado> Items { get; init; } = [];

    // Totales sobre TODOS los activos, sin aplicar el filtro de estado:
    // son los que se muestran en el subtítulo de la página, independientes
    // de qué chip esté seleccionado.
    public int Total { get; init; }
    public int Vencidos { get; init; }
    public int PorVencer { get; init; }

    public EstadoSemaforo? FiltroEstado { get; init; }
    public int? FiltroCategoriaId { get; init; }
    public string? FiltroTexto { get; init; }

    public IEnumerable<SelectListItem> Categorias { get; init; } = [];
}
