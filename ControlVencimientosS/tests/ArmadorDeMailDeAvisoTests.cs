using ControlVencimientosP.Services;

namespace ControlVencimientos.Tests;

public class ArmadorDeMailDeAvisoTests
{
    private static readonly DateOnly Hoy = new(2026, 8, 22);

    [Fact]
    public void El_asunto_del_digest_usa_singular_con_un_solo_item()
    {
        var mail = ArmadorDeMailDeAviso.ArmarDigest([new ItemParaMail(1, "Matafuego", "182", 7)], Hoy);

        Assert.Equal("Vencimientos: 1 pendiente", mail.Asunto);
    }

    [Fact]
    public void El_asunto_del_digest_usa_plural_con_varios_items()
    {
        var items = new[] { new ItemParaMail(1, "A", null, 7), new ItemParaMail(2, "B", null, 0) };
        var mail = ArmadorDeMailDeAviso.ArmarDigest(items, Hoy);

        Assert.Equal("Vencimientos: 2 pendientes", mail.Asunto);
    }

    [Fact]
    public void La_alerta_individual_nombra_el_item_cuando_es_uno_solo()
    {
        var mail = ArmadorDeMailDeAviso.ArmarAlertaIndividual([new ItemParaMail(1, "Matafuego Nº 182", "182", 0)], Hoy);

        Assert.Equal("Vencimiento urgente: Matafuego Nº 182", mail.Asunto);
    }

    [Fact]
    public void La_alerta_individual_resume_la_cantidad_cuando_son_varios()
    {
        var items = new[] { new ItemParaMail(1, "A", null, 0), new ItemParaMail(2, "B", null, -1) };
        var mail = ArmadorDeMailDeAviso.ArmarAlertaIndividual(items, Hoy);

        Assert.Equal("2 vencimientos urgentes", mail.Asunto);
    }

    [Fact]
    public void HTML_encodea_el_nombre_y_el_codigo_del_item()
    {
        var mail = ArmadorDeMailDeAviso.ArmarDigest([new ItemParaMail(1, "<script>alert(1)</script>", "<b>1</b>", 0)], Hoy);

        Assert.DoesNotContain("<script>", mail.CuerpoHtml);
        Assert.Contains("&lt;script&gt;", mail.CuerpoHtml);
        Assert.Contains("&lt;b&gt;1&lt;/b&gt;", mail.CuerpoHtml);
    }

    [Fact]
    public void El_cuerpo_ordena_los_items_de_mas_vencido_a_menos_urgente()
    {
        var items = new[] { new ItemParaMail(1, "Vence en 30", null, 30), new ItemParaMail(2, "Vencido", null, -5) };
        var mail = ArmadorDeMailDeAviso.ArmarDigest(items, Hoy);

        Assert.True(mail.CuerpoHtml.IndexOf("Vencido", StringComparison.Ordinal)
            < mail.CuerpoHtml.IndexOf("Vence en 30", StringComparison.Ordinal));
    }

    [Fact]
    public void El_item_sin_codigo_no_agrega_el_simbolo_de_numero()
    {
        var mail = ArmadorDeMailDeAviso.ArmarDigest([new ItemParaMail(1, "Contrato", null, 0)], Hoy);

        Assert.DoesNotContain("Nº", mail.CuerpoHtml);
    }
}
