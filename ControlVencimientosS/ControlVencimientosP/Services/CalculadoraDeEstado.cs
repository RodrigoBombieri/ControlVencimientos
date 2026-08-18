using ControlVencimientosP.Domain;

namespace ControlVencimientosP.Services;

/// <summary>
/// El semaforo, como funcion pura. Sin base de datos, sin DateTime.Now adentro:
/// el "hoy" entra por parametro. Por eso se puede testear de verdad, y por eso
/// los tests de este archivo son los que mas valen la pena en todo el proyecto.
/// </summary>
public static class CalculadoraDeEstado
{
    /// <summary>Dias hasta el vencimiento. Negativo = ya vencio.</summary>
    public static int DiasRestantes(DateOnly fechaVencimiento, DateOnly hoy)
        => fechaVencimiento.DayNumber - hoy.DayNumber;

    /// <summary>
    /// Verde / amarillo / rojo. Nunca se persiste: si se guardara en una
    /// columna, a la medianoche quedaria mintiendo.
    /// </summary>
    public static EstadoSemaforo Calcular(DateOnly fechaVencimiento, int diasAviso, DateOnly hoy)
    {
        var dias = DiasRestantes(fechaVencimiento, hoy);

        if (dias < 0) return EstadoSemaforo.Vencido;
        if (dias <= diasAviso) return EstadoSemaforo.PorVencer;
        return EstadoSemaforo.Vigente;
    }

    /// <summary>
    /// El vencimiento puede pisar la anticipacion de su categoria.
    /// Si no la pisa, manda la de la categoria.
    /// </summary>
    public static int DiasAvisoEfectivo(int? diasAvisoDelVencimiento, int diasAvisoDeLaCategoria)
        => diasAvisoDelVencimiento ?? diasAvisoDeLaCategoria;

    /// <summary>
    /// Que dia es "hoy" para esta empresa. Todo el calculo del semaforo pasa por
    /// aca: comparar contra UTC hace que a la noche los vencimientos cambien de
    /// color antes de tiempo.
    /// </summary>
    public static DateOnly HoyEnLaEmpresa(string zonaHoraria, DateTimeOffset ahora)
    {
        TimeZoneInfo zona;
        try
        {
            zona = TimeZoneInfo.FindSystemTimeZoneById(zonaHoraria);
        }
        catch (Exception e) when (e is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            zona = TimeZoneInfo.Utc;
        }

        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(ahora, zona).DateTime);
    }
}
