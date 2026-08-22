namespace ControlVencimientosP.Services;

/// <summary>
/// Traduce un <see cref="EstadoSemaforo"/> a lo que necesita la UI para
/// pintar el badge (clase CSS, ícono, texto) y una fecha relativa en
/// palabras ("vence hoy", "en 3 días", "hace 2 días"). Centralizado acá
/// para no repetir el mismo switch en cada vista que muestra el semáforo
/// (dashboard, listado, y lo que siga).
/// </summary>
public static class PresentacionSemaforo
{
    public static (string Clase, string Icono, string Texto) Info(EstadoSemaforo estado) => estado switch
    {
        EstadoSemaforo.Vencido => ("cv-b-crit", "alert", "Vencido"),
        EstadoSemaforo.PorVencer => ("cv-b-warn", "clock", "Por vencer"),
        _ => ("cv-b-good", "check", "Vigente")
    };

    /// <summary>
    /// La clase de color para un ".cv-dot" (el puntito que acompaña un
    /// ítem en el calendario). Separada de <see cref="Info"/> porque ese
    /// devuelve clases de badge ("cv-b-*"), no de punto ("cv-t-*").
    /// </summary>
    public static string ClaseDeDot(EstadoSemaforo estado) => estado switch
    {
        EstadoSemaforo.Vencido => "cv-t-crit",
        EstadoSemaforo.PorVencer => "cv-t-warn",
        _ => "cv-t-good"
    };

    public static string Relativo(int dias) => dias switch
    {
        < 0 => $"hace {-dias} día{(-dias == 1 ? "" : "s")}",
        0 => "vence hoy",
        1 => "en 1 día",
        _ => $"en {dias} días"
    };
}
