using ControlVencimientosP.Domain;
using ControlVencimientosP.Services;

namespace ControlVencimientos.Tests;

public class ArmadorDeEnviosTests
{
    private static readonly int[] Hitos = [30, 7, 0];
    private static readonly DateOnly Hoy = new(2026, 8, 22);

    private static FilaVencimientoAviso Fila(int diasHastaVencer, int vencimientoId = 1, int categoriaId = 1, string nombre = "Item") =>
        new(vencimientoId, nombre, null, categoriaId, Hoy.AddDays(diasHastaVencer));

    private static Destinatario Dest(int id = 1, string? email = "a@a.com", string? categorias = null) =>
        new() { Id = id, Email = email, CategoriasFiltro = categorias, Activo = true };

    private static readonly HashSet<AvisoClave> SinEnvios = [];

    [Fact]
    public void Un_destinatario_sin_email_no_genera_ningun_envio()
    {
        var envios = ArmadorDeEnvios.Armar(
            [Fila(7)], [Dest(email: null)], Hoy, Hitos, 7, digestActivo: true, alertaIndividualActiva: true, SinEnvios);

        Assert.Empty(envios);
    }

    [Fact]
    public void No_incluye_items_que_no_caen_en_un_hito()
    {
        var envios = ArmadorDeEnvios.Armar(
            [Fila(15)], [Dest()], Hoy, Hitos, 7, digestActivo: true, alertaIndividualActiva: true, SinEnvios);

        Assert.Empty(envios);
    }

    [Fact]
    public void Respeta_el_filtro_de_categorias_del_destinatario()
    {
        var filas = new[] { Fila(0, vencimientoId: 1, categoriaId: 5), Fila(0, vencimientoId: 2, categoriaId: 9) };
        var destinatario = Dest(categorias: "5");

        var envios = ArmadorDeEnvios.Armar(
            filas, [destinatario], Hoy, Hitos, 7, digestActivo: true, alertaIndividualActiva: false, SinEnvios);

        var items = Assert.Single(envios).Items;
        Assert.Equal(1, Assert.Single(items).VencimientoId);
    }

    [Fact]
    public void No_repite_un_aviso_que_ya_se_mando()
    {
        var fila = Fila(0, vencimientoId: 3);
        var destinatario = Dest(id: 2);
        var yaEnviados = new HashSet<AvisoClave> { new(3, 0, CanalAviso.Email, 2) };

        var envios = ArmadorDeEnvios.Armar(
            [fila], [destinatario], Hoy, Hitos, 7, digestActivo: true, alertaIndividualActiva: true, yaEnviados);

        Assert.Empty(envios);
    }

    [Fact]
    public void Separa_lo_urgente_en_una_alerta_individual_aparte_del_digest()
    {
        var filas = new[] { Fila(0, vencimientoId: 1), Fila(30, vencimientoId: 2) };

        var envios = ArmadorDeEnvios.Armar(
            filas, [Dest()], Hoy, Hitos, 7, digestActivo: true, alertaIndividualActiva: true, SinEnvios);

        Assert.Equal(2, envios.Count);

        var alerta = Assert.Single(envios, e => e.EsAlertaIndividual);
        Assert.Equal(1, Assert.Single(alerta.Items).VencimientoId);

        var digest = Assert.Single(envios, e => !e.EsAlertaIndividual);
        Assert.Equal(2, Assert.Single(digest.Items).VencimientoId);
    }

    [Fact]
    public void Con_alerta_individual_apagada_lo_urgente_cae_en_el_digest()
    {
        // Que el canal de alerta individual este desactivado no puede hacer
        // que un vencimiento urgente no se avise nunca.
        var filas = new[] { Fila(0, vencimientoId: 1) };

        var envios = ArmadorDeEnvios.Armar(
            filas, [Dest()], Hoy, Hitos, 7, digestActivo: true, alertaIndividualActiva: false, SinEnvios);

        var envio = Assert.Single(envios);
        Assert.False(envio.EsAlertaIndividual);
        Assert.Equal(1, Assert.Single(envio.Items).VencimientoId);
    }

    [Fact]
    public void Con_digest_apagado_lo_no_urgente_no_se_manda_por_ningun_lado()
    {
        var filas = new[] { Fila(30, vencimientoId: 1) };

        var envios = ArmadorDeEnvios.Armar(
            filas, [Dest()], Hoy, Hitos, 7, digestActivo: false, alertaIndividualActiva: true, SinEnvios);

        Assert.Empty(envios);
    }

    [Fact]
    public void Cada_destinatario_recibe_su_propio_envio()
    {
        var filas = new[] { Fila(0, vencimientoId: 1) };
        var destinatarios = new[] { Dest(id: 1, email: "uno@a.com"), Dest(id: 2, email: "dos@a.com") };

        var envios = ArmadorDeEnvios.Armar(
            filas, destinatarios, Hoy, Hitos, 7, digestActivo: true, alertaIndividualActiva: false, SinEnvios);

        Assert.Equal(2, envios.Count);
        Assert.Contains(envios, e => e.DestinatarioId == 1);
        Assert.Contains(envios, e => e.DestinatarioId == 2);
    }
}
