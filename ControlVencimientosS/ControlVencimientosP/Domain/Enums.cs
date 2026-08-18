namespace ControlVencimientosP.Domain;

/// <summary>
/// El semaforo que ve el usuario. Se CALCULA a partir de la fecha, nunca se
/// persiste: una columna con este valor se desincroniza a la medianoche.
/// </summary>
public enum EstadoSemaforo
{
    Vigente = 0,
    PorVencer = 1,
    Vencido = 2
}

/// <summary>Ciclo de vida del registro de vencimiento. Esto si se persiste.</summary>
public enum EstadoVencimiento
{
    /// <summary>El vigente. Solo puede haber uno por Item (indice unico filtrado).</summary>
    Activo = 0,
    /// <summary>Fue reemplazado por uno nuevo. Queda como historial.</summary>
    Renovado = 1,
    /// <summary>Cargado por error.</summary>
    Anulado = 2
}

public enum CanalAviso
{
    Email = 0,
    WhatsApp = 1
}

public enum EstadoAviso
{
    Enviado = 0,
    Fallido = 1
}
