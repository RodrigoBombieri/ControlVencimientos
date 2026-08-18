using ControlVencimientosP.Domain;
using ControlVencimientosP.Services;

namespace ControlVencimientos.Tests;

/// <summary>
/// Los bordes del semaforo. Son pocos tests y aburridos, y son exactamente
/// donde aparecen los bugs: el dia que vence, el dia siguiente, y el limite
/// del aviso.
/// </summary>
public class CalculadoraDeEstadoTests
{
    private static readonly DateOnly Hoy = new(2026, 8, 17);

    [Theory]
    [InlineData("2026-09-30", 30, EstadoSemaforo.Vigente)]    // faltan 44 dias
    [InlineData("2026-09-16", 30, EstadoSemaforo.PorVencer)]  // faltan exactamente 30
    [InlineData("2026-09-17", 30, EstadoSemaforo.Vigente)]    // faltan 31: uno mas que el aviso
    [InlineData("2026-08-24", 30, EstadoSemaforo.PorVencer)]  // faltan 7
    [InlineData("2026-08-17", 30, EstadoSemaforo.PorVencer)]  // vence hoy
    [InlineData("2026-08-16", 30, EstadoSemaforo.Vencido)]    // vencio ayer
    [InlineData("2026-07-12", 30, EstadoSemaforo.Vencido)]    // hace 36 dias
    public void Calcula_el_semaforo(string fecha, int diasAviso, EstadoSemaforo esperado)
    {
        var resultado = CalculadoraDeEstado.Calcular(DateOnly.Parse(fecha), diasAviso, Hoy);
        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public void El_dia_que_vence_esta_por_vencer_y_no_vencido()
    {
        // El borde que mas se equivoca: "vence hoy" todavia es amarillo, no rojo.
        Assert.Equal(EstadoSemaforo.PorVencer, CalculadoraDeEstado.Calcular(Hoy, 30, Hoy));
    }

    [Fact]
    public void Un_dia_despues_del_limite_de_aviso_sigue_vigente()
    {
        // Con aviso a 30 dias: a 30 dias es amarillo, a 31 todavia es verde.
        var justoEnElLimite = Hoy.AddDays(30);
        var unoMas = Hoy.AddDays(31);

        Assert.Equal(EstadoSemaforo.PorVencer, CalculadoraDeEstado.Calcular(justoEnElLimite, 30, Hoy));
        Assert.Equal(EstadoSemaforo.Vigente, CalculadoraDeEstado.Calcular(unoMas, 30, Hoy));
    }

    [Theory]
    [InlineData("2026-08-24", 7)]
    [InlineData("2026-08-17", 0)]
    [InlineData("2026-08-16", -1)]
    [InlineData("2026-07-12", -36)]
    public void Cuenta_bien_los_dias_restantes(string fecha, int esperado)
    {
        Assert.Equal(esperado, CalculadoraDeEstado.DiasRestantes(DateOnly.Parse(fecha), Hoy));
    }

    [Fact]
    public void El_vencimiento_pisa_la_anticipacion_de_la_categoria()
    {
        Assert.Equal(15, CalculadoraDeEstado.DiasAvisoEfectivo(15, 30));
        Assert.Equal(30, CalculadoraDeEstado.DiasAvisoEfectivo(null, 30));
    }

    [Fact]
    public void Hoy_se_calcula_en_la_zona_horaria_de_la_empresa()
    {
        // 18/08 a las 02:00 UTC todavia es 17/08 en Buenos Aires (UTC-3).
        // Comparar contra UTC haria que los vencimientos cambien de color de noche.
        var ahora = new DateTimeOffset(2026, 8, 18, 2, 0, 0, TimeSpan.Zero);

        var hoy = CalculadoraDeEstado.HoyEnLaEmpresa("America/Argentina/Buenos_Aires", ahora);

        Assert.Equal(new DateOnly(2026, 8, 17), hoy);
    }

    [Fact]
    public void Una_zona_horaria_invalida_no_tira_excepcion()
    {
        var ahora = new DateTimeOffset(2026, 8, 18, 2, 0, 0, TimeSpan.Zero);
        var hoy = CalculadoraDeEstado.HoyEnLaEmpresa("Zona/Inexistente", ahora);

        Assert.Equal(new DateOnly(2026, 8, 18), hoy);   // cae a UTC
    }
}
