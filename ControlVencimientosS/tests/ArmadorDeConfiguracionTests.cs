using ControlVencimientosP.Services;

namespace ControlVencimientos.Tests;

public class ArmadorDeConfiguracionTests
{
    [Fact]
    public void Ordena_de_mayor_a_menor_sin_importar_como_se_tildaron_las_pills()
    {
        var resultado = ArmadorDeConfiguracion.ArmarHitos([7, 60, 1, 30]);

        Assert.True(resultado.EsValido);
        Assert.Equal("60,30,7,1", resultado.HitosDias);
    }

    [Fact]
    public void Descarta_los_repetidos()
    {
        var resultado = ArmadorDeConfiguracion.ArmarHitos([30, 30, 7, 7, 0]);

        Assert.True(resultado.EsValido);
        Assert.Equal("30,7,0", resultado.HitosDias);
    }

    [Fact]
    public void Un_solo_hito_seleccionado_no_deja_una_coma_colgando()
    {
        var resultado = ArmadorDeConfiguracion.ArmarHitos([15]);

        Assert.True(resultado.EsValido);
        Assert.Equal("15", resultado.HitosDias);
    }

    [Fact]
    public void Sin_ningun_hito_seleccionado_es_invalido()
    {
        var resultado = ArmadorDeConfiguracion.ArmarHitos([]);

        Assert.False(resultado.EsValido);
        Assert.Equal("", resultado.HitosDias);
    }

    [Fact]
    public void Ignora_valores_que_no_estan_en_el_preset_disponible()
    {
        // Alguien posteando el formulario a mano, fuera del checklist real del
        // navegador: un 12 no es una de las pills, no tiene que colarse.
        var resultado = ArmadorDeConfiguracion.ArmarHitos([60, 12, 30]);

        Assert.True(resultado.EsValido);
        Assert.Equal("60,30", resultado.HitosDias);
    }

    [Fact]
    public void Si_todos_los_valores_son_ajenos_al_preset_queda_invalido()
    {
        var resultado = ArmadorDeConfiguracion.ArmarHitos([12, 200, -5]);

        Assert.False(resultado.EsValido);
        Assert.Equal("", resultado.HitosDias);
    }

    [Fact]
    public void Acepta_un_icono_del_preset()
    {
        Assert.Equal("🧯", ArmadorDeConfiguracion.ArmarIcono("🧯"));
    }

    [Theory]
    [InlineData((string?)null)]
    [InlineData("")]
    [InlineData("🐙")]
    public void Cae_al_default_si_el_icono_no_esta_en_el_preset(string? posteado)
    {
        Assert.Equal(ArmadorDeConfiguracion.IconoPorDefecto, ArmadorDeConfiguracion.ArmarIcono(posteado));
    }

    private static FilaDestinatario Fila(int id, string nombre, string? email, string? telefono, IReadOnlyList<int> categorias, bool activo = true) =>
        new(id, nombre, email, telefono, categorias, activo);

    [Fact]
    public void Sin_categorias_de_filtro_dice_todas_las_categorias()
    {
        var filas = new[] { Fila(1, "Rodrigo", "rodrigo@x.com", null, []) };

        var resultado = ArmadorDeConfiguracion.ArmarDestinatarios(filas, new Dictionary<int, string>());

        Assert.Equal("todas las categorías", resultado.Single().CategoriasResumen);
    }

    [Fact]
    public void Con_categorias_de_filtro_arma_el_resumen_con_los_nombres()
    {
        var nombres = new Dictionary<int, string> { [1] = "Matafuego", [3] = "ART", [8] = "Capacitación" };
        var filas = new[] { Fila(1, "Seguridad e Higiene", "seg@x.com", null, [1, 3, 8]) };

        var resultado = ArmadorDeConfiguracion.ArmarDestinatarios(filas, nombres);

        Assert.Equal("Matafuego, ART, Capacitación", resultado.Single().CategoriasResumen);
    }

    [Fact]
    public void Una_categoria_que_ya_no_existe_se_omite_del_resumen_sin_romper()
    {
        var nombres = new Dictionary<int, string> { [1] = "Matafuego" };
        var filas = new[] { Fila(1, "Alguien", "a@x.com", null, [1, 99]) };

        var resultado = ArmadorDeConfiguracion.ArmarDestinatarios(filas, nombres);

        Assert.Equal("Matafuego", resultado.Single().CategoriasResumen);
    }

    [Fact]
    public void Junta_email_y_telefono_con_un_separador()
    {
        var filas = new[] { Fila(1, "Rodrigo", "rodrigo@x.com", "11-2222-3333", []) };

        var resultado = ArmadorDeConfiguracion.ArmarDestinatarios(filas, new Dictionary<int, string>());

        Assert.Equal("rodrigo@x.com · 11-2222-3333", resultado.Single().Contacto);
    }

    [Fact]
    public void Con_un_solo_dato_de_contacto_no_deja_el_separador_colgando()
    {
        var filas = new[] { Fila(1, "Rodrigo", "rodrigo@x.com", null, []) };

        var resultado = ArmadorDeConfiguracion.ArmarDestinatarios(filas, new Dictionary<int, string>());

        Assert.Equal("rodrigo@x.com", resultado.Single().Contacto);
    }

    [Fact]
    public void Pasa_de_largo_el_id_y_el_estado_activo()
    {
        var filas = new[] { Fila(42, "Rodrigo", "rodrigo@x.com", null, [], activo: false) };

        var resultado = ArmadorDeConfiguracion.ArmarDestinatarios(filas, new Dictionary<int, string>()).Single();

        Assert.Equal(42, resultado.Id);
        Assert.False(resultado.Activo);
    }
}
