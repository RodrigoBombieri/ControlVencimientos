namespace ControlVencimientosP.Services;

/// <summary>Resultado de armar los hitos de aviso elegidos en el formulario.</summary>
public record ResultadoHitos(bool EsValido, string HitosDias);

/// <summary>Una fila cruda de destinatario, tal como sale de la base.</summary>
public record FilaDestinatario(
    int Id, string Nombre, string? Email, string? Telefono, IReadOnlyList<int> CategoriasFiltro, bool Activo);

/// <summary>Un destinatario ya listo para pintar en la pantalla de Configuración.</summary>
public record DestinatarioFila(
    int Id, string Nombre, string Contacto, string CategoriasResumen, bool Activo);

/// <summary>Una categoría ya lista para pintar en la pantalla de Configuración.</summary>
public record CategoriaFila(
    int Id, string Nombre, string Icono, int DiasAvisoDefault, int CantidadItems, bool Activa);

/// <summary>
/// Funciones puras para la pantalla de Configuración: parsear lo que escribe
/// el usuario y traducir filas crudas de la base a algo legible.
/// </summary>
public static class ArmadorDeConfiguracion
{
    /// <summary>
    /// El preset fijo de hitos que se muestra como pills en Configuración.
    /// El usuario no escribe días a mano: elige de esta lista, asi que nunca
    /// puede terminar con un hito mal escrito.
    /// </summary>
    public static readonly IReadOnlyList<int> HitosDisponibles = [90, 60, 45, 30, 15, 7, 3, 1, 0];

    /// <summary>
    /// El preset fijo de emojis que se puede elegir como ícono de una
    /// categoría. Un selector libre (subir una imagen, un color picker de
    /// SVGs) es de otro presupuesto; con un set fijo alcanza para distinguir
    /// categorías a simple vista sin agregar ni una dependencia nueva.
    /// </summary>
    public static readonly IReadOnlyList<string> IconosDisponibles =
    [
        "🏢", "🧯", "🛡️", "🦺", "🎓", "🪪", "📜", "🔧", "🔍",
        "📄", "🗂️", "⚡", "🚗", "💧", "🏥", "🧪", "🛗", "❄️", "📦", "🔒", "🔥"
    ];

    /// <summary>El ícono con el que arranca una categoría nueva antes de que el usuario elija uno.</summary>
    public const string IconoPorDefecto = "📄";

    /// <summary>
    /// Valida el ícono que posteó el formulario. Si viene vacío o con algo
    /// fuera del preset (alguien posteando a mano), cae al default en vez de
    /// rechazar el guardado entero: a diferencia de los hitos, el ícono es
    /// cosmético, no vale la pena bloquear el formulario por esto.
    /// </summary>
    public static string ArmarIcono(string? seleccionado)
        => seleccionado is not null && IconosDisponibles.Contains(seleccionado)
            ? seleccionado
            : IconoPorDefecto;

    /// <summary>
    /// Arma el <see cref="ConfiguracionAvisos.HitosDias"/> a partir de los
    /// dias que tildo el usuario en el checklist de pills. Se descarta
    /// cualquier valor que no este en <see cref="HitosDisponibles"/> (alguien
    /// posteando el formulario a mano, fuera del navegador) y hace falta
    /// elegir al menos uno: sin hitos no hay cuando avisar.
    /// </summary>
    public static ResultadoHitos ArmarHitos(IEnumerable<int> seleccionados)
    {
        var hitos = seleccionados
            .Where(HitosDisponibles.Contains)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        if (hitos.Count == 0)
            return new ResultadoHitos(false, "");

        return new ResultadoHitos(true, string.Join(",", hitos));
    }

    /// <summary>
    /// Arma la fila de cada destinatario para la pantalla: junta el email y
    /// el telefono en un solo texto de contacto, y traduce los ids de
    /// <see cref="Destinatario.CategoriasFiltro"/> a nombres legibles.
    /// </summary>
    public static IReadOnlyList<DestinatarioFila> ArmarDestinatarios(
        IEnumerable<FilaDestinatario> filas, IReadOnlyDictionary<int, string> nombrePorCategoriaId)
    {
        return filas.Select(f =>
        {
            var contacto = string.Join(" · ", new[] { f.Email, f.Telefono }.Where(v => !string.IsNullOrWhiteSpace(v)));

            var resumenCategorias = f.CategoriasFiltro.Count == 0
                ? "todas las categorías"
                : string.Join(", ", f.CategoriasFiltro
                    .Select(id => nombrePorCategoriaId.GetValueOrDefault(id))
                    .Where(nombre => nombre is not null));

            return new DestinatarioFila(f.Id, f.Nombre, contacto, resumenCategorias, f.Activo);
        }).ToList();
    }
}
