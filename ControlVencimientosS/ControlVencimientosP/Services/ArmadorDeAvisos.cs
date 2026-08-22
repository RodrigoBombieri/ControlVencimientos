namespace ControlVencimientosP.Services;

/// <summary>Un item que el proximo envio automatico mencionaria.</summary>
public record ItemAviso(int VencimientoId, string Nombre, int DiasRestantes);

/// <summary>
/// Vista previa del proximo envio automatico. No es un mock: usa
/// <see cref="ReglaDeAvisos"/>, la misma regla que algun dia va a ejecutar el
/// job de verdad, contra los vencimientos activos de hoy. Lo unico que todavia
/// no existe es el job de Hangfire que la dispara sola.
/// </summary>
public class ProximoAviso
{
    public DateOnly Fecha { get; init; }
    public TimeOnly Hora { get; init; }

    /// <summary>True si el proximo envio es hoy (todavia no paso la hora); false si es manana.</summary>
    public bool EsHoy { get; init; }

    public int CantidadDestinatarios { get; init; }

    /// <summary>Cuantos vencimientos entrarian en total, no solo los urgentes.</summary>
    public int TotalItems { get; init; }

    /// <summary>Los mas urgentes primero, para mostrar en el preview del mail.</summary>
    public IReadOnlyList<ItemAviso> Urgentes { get; init; } = [];
}

/// <summary>Arma la vista previa del proximo envio automatico. Funcion pura.</summary>
public static class ArmadorDeAvisos
{
    public static ProximoAviso Armar(
        IEnumerable<FilaVencimiento> filas,
        DateOnly hoy,
        TimeOnly horaActual,
        TimeOnly horaEnvio,
        IReadOnlyCollection<int> hitos,
        int reenviarVencidosCadaDias,
        int cantidadDestinatarios,
        int maxUrgentes = 3)
    {
        // Si ya paso la hora de hoy, el proximo envio es manana a la misma hora.
        var esHoy = horaActual < horaEnvio;
        var fecha = esHoy ? hoy : hoy.AddDays(1);

        var incluidos = filas
            .Select(f => new ItemAviso(
                f.VencimientoId,
                f.ItemNombre,
                CalculadoraDeEstado.DiasRestantes(f.FechaVencimiento, fecha)))
            .Where(i => ReglaDeAvisos.CorrespondeAvisar(i.DiasRestantes, hitos, reenviarVencidosCadaDias))
            .ToList();

        var urgentes = incluidos
            .Where(i => ReglaDeAvisos.EsUrgente(i.DiasRestantes))
            .OrderBy(i => i.DiasRestantes)
            .Take(maxUrgentes)
            .ToList();

        return new ProximoAviso
        {
            Fecha = fecha,
            Hora = horaEnvio,
            EsHoy = esHoy,
            CantidadDestinatarios = cantidadDestinatarios,
            TotalItems = incluidos.Count,
            Urgentes = urgentes
        };
    }
}
