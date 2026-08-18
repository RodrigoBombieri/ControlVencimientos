using ControlVencimientosP.Domain;
using ControlVencimientosP.Services;

namespace ControlVencimientos.Tests;

public class ReglaDeAvisosTests
{
    private static readonly int[] Hitos = [60, 30, 15, 7, 1, 0];

    [Theory]
    [InlineData(60, true)]
    [InlineData(30, true)]
    [InlineData(7, true)]
    [InlineData(1, true)]
    [InlineData(0, true)]    // el dia que vence
    [InlineData(45, false)]
    [InlineData(2, false)]
    [InlineData(31, false)]
    public void Avisa_solo_en_los_hitos_configurados(int diasRestantes, bool esperado)
    {
        Assert.Equal(esperado, ReglaDeAvisos.CorrespondeAvisar(diasRestantes, Hitos, 7));
    }

    [Theory]
    [InlineData(-7, true)]    // una semana vencido
    [InlineData(-14, true)]
    [InlineData(-21, true)]
    [InlineData(-1, false)]   // al dia siguiente no se insiste
    [InlineData(-6, false)]
    [InlineData(-8, false)]
    public void Recuerda_lo_vencido_cada_siete_dias(int diasRestantes, bool esperado)
    {
        Assert.Equal(esperado, ReglaDeAvisos.CorrespondeAvisar(diasRestantes, Hitos, 7));
    }

    [Fact]
    public void Con_periodo_cero_no_recuerda_lo_vencido()
    {
        Assert.False(ReglaDeAvisos.CorrespondeAvisar(-7, Hitos, 0));
        Assert.False(ReglaDeAvisos.CorrespondeAvisar(-70, Hitos, 0));
    }

    [Theory]
    [InlineData(7, true)]
    [InlineData(0, true)]
    [InlineData(-3, true)]
    [InlineData(8, false)]
    [InlineData(30, false)]
    public void Lo_de_siete_dias_o_menos_es_urgente(int diasRestantes, bool esperado)
    {
        // Lo urgente va en un mail individual; el resto se agrupa en el digest.
        Assert.Equal(esperado, ReglaDeAvisos.EsUrgente(diasRestantes));
    }

    [Fact]
    public void Los_hitos_se_parsean_de_la_configuracion()
    {
        var config = new ConfiguracionAvisos { HitosDias = " 30, 7 ,7, 0 , basura , -5 " };

        Assert.Equal([30, 7, 0], config.Hitos());
    }

    [Fact]
    public void Un_destinatario_sin_filtro_recibe_todas_las_categorias()
    {
        var todos = new Destinatario { CategoriasFiltro = null };
        var soloAlgunas = new Destinatario { CategoriasFiltro = "1,3,8" };

        Assert.True(todos.CubreCategoria(5));
        Assert.True(soloAlgunas.CubreCategoria(3));
        Assert.False(soloAlgunas.CubreCategoria(5));
    }
}
