using ControlVencimientosP.Domain;
using ControlVencimientosP.Services;

namespace ControlVencimientos.Tests;

public class ArmadorDeListadoTests
{
    private static readonly DateOnly Hoy = new(2026, 8, 21);

    private static FilaListado Fila(
        int id, string fecha, string nombre = "Item", string? codigo = null, string? ubicacion = null,
        int categoriaId = 1, string categoriaNombre = "Categoria", int diasAvisoCat = 30) =>
        new(id, id, nombre, codigo, ubicacion, categoriaId, categoriaNombre, "flame", DateOnly.Parse(fecha), null, diasAvisoCat);

    [Fact]
    public void Sin_filtros_devuelve_todo_ordenado_por_fecha()
    {
        var filas = new[]
        {
            Fila(1, "2026-12-01", "El de diciembre"),
            Fila(2, "2026-07-01", "El vencido"),
            Fila(3, "2026-08-25", "El de agosto"),
        };

        var calculado = ArmadorDeListado.Calcular(filas, Hoy);
        var filtrado = ArmadorDeListado.Filtrar(calculado);

        Assert.Equal(["El vencido", "El de agosto", "El de diciembre"], filtrado.Select(i => i.Nombre));
    }

    [Fact]
    public void Filtra_por_estado()
    {
        var filas = new[]
        {
            Fila(1, "2026-07-01"),   // vencido
            Fila(2, "2026-08-25"),   // por vencer
            Fila(3, "2026-12-01"),   // vigente
        };

        var calculado = ArmadorDeListado.Calcular(filas, Hoy);
        var soloVencidos = ArmadorDeListado.Filtrar(calculado, estado: EstadoSemaforo.Vencido);

        Assert.Single(soloVencidos);
        Assert.Equal(EstadoSemaforo.Vencido, soloVencidos[0].Estado);
    }

    [Fact]
    public void Filtra_por_categoria()
    {
        var filas = new[]
        {
            Fila(1, "2026-12-01", categoriaId: 1),
            Fila(2, "2026-12-02", categoriaId: 2),
        };

        var calculado = ArmadorDeListado.Calcular(filas, Hoy);
        var filtrado = ArmadorDeListado.Filtrar(calculado, categoriaId: 2);

        Assert.Single(filtrado);
        Assert.Equal(2, filtrado[0].CategoriaId);
    }

    [Theory]
    [InlineData("matafuego")]   // nombre
    [InlineData("MATA")]        // nombre, sin importar mayusculas
    [InlineData("182")]         // codigo
    [InlineData("depos")]       // ubicacion
    public void Filtra_por_texto_en_nombre_codigo_o_ubicacion(string texto)
    {
        var filas = new[]
        {
            Fila(1, "2026-12-01", nombre: "Matafuego Nro 182", codigo: "182", ubicacion: "Deposito central"),
            Fila(2, "2026-12-02", nombre: "Habilitacion municipal", codigo: "H-1", ubicacion: "Oficina"),
        };

        var calculado = ArmadorDeListado.Calcular(filas, Hoy);
        var filtrado = ArmadorDeListado.Filtrar(calculado, texto: texto);

        Assert.Single(filtrado);
        Assert.Equal(1, filtrado[0].ItemId);
    }

    [Fact]
    public void Combina_filtros_con_AND()
    {
        var filas = new[]
        {
            Fila(1, "2026-07-01", nombre: "Matafuego A", categoriaId: 1),  // vencido, categoria 1
            Fila(2, "2026-07-02", nombre: "Matafuego B", categoriaId: 2),  // vencido, categoria 2
            Fila(3, "2026-12-01", nombre: "Matafuego C", categoriaId: 1),  // vigente, categoria 1
        };

        var calculado = ArmadorDeListado.Calcular(filas, Hoy);
        var filtrado = ArmadorDeListado.Filtrar(calculado, estado: EstadoSemaforo.Vencido, categoriaId: 1, texto: "matafuego");

        Assert.Single(filtrado);
        Assert.Equal(1, filtrado[0].ItemId);
    }

    [Fact]
    public void Filtrar_sobre_lista_vacia_no_rompe()
    {
        var filtrado = ArmadorDeListado.Filtrar(ArmadorDeListado.Calcular([], Hoy), estado: EstadoSemaforo.Vencido);

        Assert.Empty(filtrado);
    }

    [Fact]
    public void Pagina_recorta_la_cantidad_pedida_y_respeta_el_orden()
    {
        var filas = Enumerable.Range(1, 25)
            .Select(i => Fila(i, DateOnly.FromDayNumber(Hoy.DayNumber + i).ToString("yyyy-MM-dd"), $"Item {i}"))
            .ToArray();

        var calculado = ArmadorDeListado.Calcular(filas, Hoy);
        var primeraPagina = ArmadorDeListado.Paginar(calculado, pagina: 1, itemsPorPagina: 10);
        var segundaPagina = ArmadorDeListado.Paginar(calculado, pagina: 2, itemsPorPagina: 10);
        var terceraPagina = ArmadorDeListado.Paginar(calculado, pagina: 3, itemsPorPagina: 10);

        Assert.Equal(10, primeraPagina.Count);
        Assert.Equal("Item 1", primeraPagina[0].Nombre);
        Assert.Equal(10, segundaPagina.Count);
        Assert.Equal("Item 11", segundaPagina[0].Nombre);
        Assert.Equal(5, terceraPagina.Count);   // 25 items, página 3 de a 10: quedan 5
    }

    [Fact]
    public void Una_pagina_invalida_se_trata_como_la_primera()
    {
        var filas = new[] { Fila(1, "2026-12-01"), Fila(2, "2026-12-02") };
        var calculado = ArmadorDeListado.Calcular(filas, Hoy);

        var conCero = ArmadorDeListado.Paginar(calculado, pagina: 0, itemsPorPagina: 10);
        var conNegativa = ArmadorDeListado.Paginar(calculado, pagina: -3, itemsPorPagina: 10);

        Assert.Equal(2, conCero.Count);
        Assert.Equal(2, conNegativa.Count);
    }

    [Fact]
    public void Pedir_una_pagina_que_no_existe_devuelve_lista_vacia()
    {
        var filas = new[] { Fila(1, "2026-12-01") };
        var calculado = ArmadorDeListado.Calcular(filas, Hoy);

        var pagina = ArmadorDeListado.Paginar(calculado, pagina: 5, itemsPorPagina: 10);

        Assert.Empty(pagina);
    }
}
