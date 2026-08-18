namespace ControlVencimientosP.Services;

/// <summary>
/// Decide si hoy hay que avisar de un vencimiento. Tambien es funcion pura,
/// tambien sin reloj adentro. Combinada con el indice unico de AvisoEnviado,
/// es todo el motor de notificaciones.
/// </summary>
public static class ReglaDeAvisos
{
    /// <summary>A partir de aca el aviso va como alerta individual, no en el digest.</summary>
    public const int DiasUrgencia = 7;

    /// <summary>
    /// True si <paramref name="diasRestantes"/> cae en un hito configurado, o si
    /// ya vencio y toca el recordatorio periodico.
    /// </summary>
    /// <param name="diasRestantes">Negativo si ya vencio.</param>
    /// <param name="hitos">Dias de anticipacion configurados. Ej: 60, 30, 15, 7, 1, 0.</param>
    /// <param name="reenviarVencidosCadaDias">Cada cuanto recordar lo vencido. 0 = no recordar.</param>
    public static bool CorrespondeAvisar(
        int diasRestantes,
        IReadOnlyCollection<int> hitos,
        int reenviarVencidosCadaDias)
    {
        if (diasRestantes >= 0)
            return hitos.Contains(diasRestantes);

        if (reenviarVencidosCadaDias <= 0)
            return false;

        // -7 con periodo 7 avisa; -8 no. Asi el recordatorio sale una vez por
        // semana en vez de todos los dias hasta que alguien lo resuelva.
        return -diasRestantes % reenviarVencidosCadaDias == 0;
    }

    /// <summary>Lo urgente va en un mail aparte; el resto se agrupa en el digest.</summary>
    public static bool EsUrgente(int diasRestantes) => diasRestantes <= DiasUrgencia;
}
