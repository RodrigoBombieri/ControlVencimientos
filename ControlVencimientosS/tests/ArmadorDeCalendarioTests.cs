using ControlVencimientosP.Domain;
using ControlVencimientosP.Services;

namespace ControlVencimientos.Tests;

public class ArmadorDeCalendarioTests
{
    // Agosto 2026: empieza sábado (5 días de relleno) y termina lunes (6 días
    // de relleno) → 6 semanas completas. Elegido porque es justo el mes que
    // usa el mockup de referencia.
    private static readonly DateOnly Hoy = new(2026, 8, 17);

    private static FilaCalendario Fila(string fecha, string nombre = "Item", int vencimientoId = 1, int? diasAvisoVenc = null, int diasAvisoCat = 30) =>
        new(vencimientoId, 1, nombre, DateOnly.Parse(fecha), diasAvisoVenc, diasAvisoCat);

    [Fact]
    public void Arma_seis_semanas_completas_para_un_mes_que_no_calza_justo()
    {
        var resumen = ArmadorDeCalendario.Armar([], 2026, 8, Hoy);

        Assert.Equal(6, resumen.Semanas.Count);
        Assert.All(resumen.Semanas, semana => Assert.Equal(7, semana.Count));
    }

    [Fact]
    public void La_grilla_arranca_en_lunes_y_termina_en_domingo()
    {
        var resumen = ArmadorDeCalendario.Armar([], 2026, 8, Hoy);

        Assert.Equal(new DateOnly(2026, 7, 27), resumen.Semanas[0][0].Fecha);
        Assert.Equal(DayOfWeek.Monday, resumen.Semanas[0][0].Fecha.DayOfWeek);

        Assert.Equal(new DateOnly(2026, 9, 6), resumen.Semanas[^1][^1].Fecha);
        Assert.Equal(DayOfWeek.Sunday, resumen.Semanas[^1][^1].Fecha.DayOfWeek);
    }

    [Fact]
    public void No_agrega_relleno_cuando_el_mes_ya_empieza_en_lunes()
    {
        // Junio 2026 empieza un lunes: el primer día de la grilla tiene que
        // ser el 1, sin ningún día de mayo antes.
        var resumen = ArmadorDeCalendario.Armar([], 2026, 6, Hoy);

        var primerDia = resumen.Semanas[0][0];
        Assert.Equal(new DateOnly(2026, 6, 1), primerDia.Fecha);
        Assert.True(primerDia.EsDelMesActual);
    }

    [Fact]
    public void No_agrega_relleno_cuando_el_mes_ya_termina_en_domingo()
    {
        // Mayo 2026 termina un domingo: el último día de la grilla tiene
        // que ser el 31, sin ningún día de junio después.
        var resumen = ArmadorDeCalendario.Armar([], 2026, 5, Hoy);

        var ultimoDia = resumen.Semanas[^1][^1];
        Assert.Equal(new DateOnly(2026, 5, 31), ultimoDia.Fecha);
        Assert.True(ultimoDia.EsDelMesActual);
    }

    [Fact]
    public void Marca_los_dias_de_relleno_como_de_otro_mes()
    {
        var resumen = ArmadorDeCalendario.Armar([], 2026, 8, Hoy);

        var diaDeJulio = resumen.Semanas[0][0]; // 27/07, relleno
        var diaDeAgosto = resumen.Semanas[0][5]; // 01/08, real (sábado)

        Assert.False(diaDeJulio.EsDelMesActual);
        Assert.True(diaDeAgosto.EsDelMesActual);
    }

    [Fact]
    public void Marca_EsHoy_solo_en_el_dia_correspondiente()
    {
        var resumen = ArmadorDeCalendario.Armar([], 2026, 8, Hoy);

        var todosLosDias = resumen.Semanas.SelectMany(s => s).ToList();
        var hoy = todosLosDias.Single(d => d.EsHoy);

        Assert.Equal(Hoy, hoy.Fecha);
        Assert.Equal(1, todosLosDias.Count(d => d.EsHoy));
    }

    [Fact]
    public void Ubica_cada_item_en_su_dia_con_el_semaforo_calculado()
    {
        var filas = new[]
        {
            Fila("2026-08-05", "Carnet vencido", vencimientoId: 10), // ya pasó, sin aviso que lo tape: vencido
            Fila("2026-08-19", "Grupo electrógeno", vencimientoId: 11, diasAvisoVenc: 7), // a 2 días, dentro del aviso: por vencer
            Fila("2026-09-01", "Contrato lejano", vencimientoId: 12, diasAvisoVenc: 5), // a más de un mes: vigente
        };

        var resumen = ArmadorDeCalendario.Armar(filas, 2026, 8, Hoy);
        var todosLosDias = resumen.Semanas.SelectMany(s => s).ToDictionary(d => d.Fecha);

        var diaVencido = todosLosDias[new DateOnly(2026, 8, 5)];
        Assert.Equal(EstadoSemaforo.Vencido, Assert.Single(diaVencido.Items).Estado);

        var diaPorVencer = todosLosDias[new DateOnly(2026, 8, 19)];
        Assert.Equal(EstadoSemaforo.PorVencer, Assert.Single(diaPorVencer.Items).Estado);

        var diaVigente = todosLosDias[new DateOnly(2026, 9, 1)];
        Assert.Equal(EstadoSemaforo.Vigente, Assert.Single(diaVigente.Items).Estado);
    }

    [Fact]
    public void Ordena_alfabeticamente_los_items_del_mismo_dia()
    {
        var filas = new[]
        {
            Fila("2026-08-24", "Matafuego Nº 183", vencimientoId: 2),
            Fila("2026-08-24", "Matafuego Nº 182", vencimientoId: 1),
        };

        var resumen = ArmadorDeCalendario.Armar(filas, 2026, 8, Hoy);
        var dia = resumen.Semanas.SelectMany(s => s).Single(d => d.Fecha == new DateOnly(2026, 8, 24));

        Assert.Equal(["Matafuego Nº 182", "Matafuego Nº 183"], dia.Items.Select(i => i.Nombre));
    }

    [Fact]
    public void Un_dia_sin_vencimientos_tiene_la_lista_de_items_vacia()
    {
        var resumen = ArmadorDeCalendario.Armar([], 2026, 8, Hoy);

        Assert.All(resumen.Semanas.SelectMany(s => s), dia => Assert.Empty(dia.Items));
    }

    [Fact]
    public void RangoDeGrilla_coincide_con_los_bordes_de_la_grilla_armada()
    {
        // Mismo mes que "La_grilla_arranca_en_lunes_y_termina_en_domingo":
        // el rango que usa el controller para la consulta tiene que ser
        // exactamente lo que despues pinta Armar, ni un dia mas ni menos.
        var (inicio, fin) = ArmadorDeCalendario.RangoDeGrilla(2026, 8);

        Assert.Equal(new DateOnly(2026, 7, 27), inicio);
        Assert.Equal(new DateOnly(2026, 9, 6), fin);
    }

    [Fact]
    public void RangoDeGrilla_sin_relleno_cuando_el_mes_ya_calza_justo()
    {
        var (inicio, fin) = ArmadorDeCalendario.RangoDeGrilla(2026, 6);

        Assert.Equal(new DateOnly(2026, 6, 1), inicio);
        Assert.Equal(DayOfWeek.Monday, inicio.DayOfWeek);
    }
}
