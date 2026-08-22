using ControlVencimientosP.Services;

namespace ControlVencimientos.Tests;

public class ArmadorDeAvisosTests
{
    private static readonly DateOnly Hoy = new(2026, 8, 21);
    private static readonly int[] Hitos = [60, 30, 15, 7, 1, 0];
    private static readonly TimeOnly HoraEnvio = new(8, 0);

    private static FilaVencimiento Fila(string fecha, string nombre = "Item") =>
        new(1, 1, nombre, "COD-1", "Categoria", "flame", DateOnly.Parse(fecha), null, 30);

    [Fact]
    public void Antes_de_la_hora_de_envio_el_proximo_aviso_es_hoy()
    {
        var aviso = ArmadorDeAvisos.Armar([], Hoy, new TimeOnly(7, 30), HoraEnvio, Hitos, 7, cantidadDestinatarios: 2);

        Assert.True(aviso.EsHoy);
        Assert.Equal(Hoy, aviso.Fecha);
    }

    [Fact]
    public void Despues_de_la_hora_de_envio_el_proximo_aviso_es_manana()
    {
        var aviso = ArmadorDeAvisos.Armar([], Hoy, new TimeOnly(9, 0), HoraEnvio, Hitos, 7, cantidadDestinatarios: 2);

        Assert.False(aviso.EsHoy);
        Assert.Equal(Hoy.AddDays(1), aviso.Fecha);
    }

    [Fact]
    public void Solo_entran_los_vencimientos_que_caen_en_un_hito()
    {
        var filas = new[]
        {
            Fila(Hoy.AddDays(15).ToString("yyyy-MM-dd"), "En un hito"),
            Fila(Hoy.AddDays(10).ToString("yyyy-MM-dd"), "Fuera de hito"),
        };

        var aviso = ArmadorDeAvisos.Armar(filas, Hoy, new TimeOnly(7, 0), HoraEnvio, Hitos, 7, cantidadDestinatarios: 1);

        Assert.Equal(1, aviso.TotalItems);
    }

    [Fact]
    public void Separa_lo_urgente_para_el_preview_y_cuenta_todo_en_el_total()
    {
        var filas = new[]
        {
            Fila(Hoy.AddDays(1).ToString("yyyy-MM-dd"), "Urgente 1"),   // hito 1: urgente
            Fila(Hoy.ToString("yyyy-MM-dd"), "Urgente 2"),              // hito 0: urgente
            Fila(Hoy.AddDays(15).ToString("yyyy-MM-dd"), "No urgente"), // hito 15: no urgente
        };

        var aviso = ArmadorDeAvisos.Armar(filas, Hoy, new TimeOnly(7, 0), HoraEnvio, Hitos, 7, cantidadDestinatarios: 1, maxUrgentes: 5);

        Assert.Equal(3, aviso.TotalItems);
        Assert.Equal(["Urgente 2", "Urgente 1"], aviso.Urgentes.Select(u => u.Nombre));
    }

    [Fact]
    public void Recorta_los_urgentes_del_preview_pero_no_el_total()
    {
        var filas = Enumerable.Range(0, 5)
            .Select(i => Fila(Hoy.ToString("yyyy-MM-dd"), $"Item {i}"))
            .ToArray();

        var aviso = ArmadorDeAvisos.Armar(filas, Hoy, new TimeOnly(7, 0), HoraEnvio, Hitos, 7, cantidadDestinatarios: 1, maxUrgentes: 3);

        Assert.Equal(5, aviso.TotalItems);
        Assert.Equal(3, aviso.Urgentes.Count);
    }

    [Fact]
    public void Pasa_de_largo_la_cantidad_de_destinatarios()
    {
        var conDestinatarios = ArmadorDeAvisos.Armar([], Hoy, new TimeOnly(7, 0), HoraEnvio, Hitos, 7, cantidadDestinatarios: 4);
        var sinDestinatarios = ArmadorDeAvisos.Armar([], Hoy, new TimeOnly(7, 0), HoraEnvio, Hitos, 7, cantidadDestinatarios: 0);

        Assert.Equal(4, conDestinatarios.CantidadDestinatarios);
        Assert.Equal(0, sinDestinatarios.CantidadDestinatarios);
    }
}
