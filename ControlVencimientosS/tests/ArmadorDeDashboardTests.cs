using ControlVencimientosP.Domain;
using ControlVencimientosP.Services;

namespace ControlVencimientos.Tests;

public class ArmadorDeDashboardTests
{
    private static readonly DateOnly Hoy = new(2026, 8, 21);

    private static FilaVencimiento Fila(string fecha, string nombre = "Item", int? diasAvisoVenc = null, int diasAvisoCat = 30) =>
        new(1, 1, nombre, "COD-1", "Categoria", "flame", DateOnly.Parse(fecha), diasAvisoVenc, diasAvisoCat);

    [Fact]
    public void Sin_filas_devuelve_todo_en_cero_y_lista_vacia()
    {
        var resumen = ArmadorDeDashboard.Armar([], Hoy);

        Assert.Equal(0, resumen.TotalActivos);
        Assert.Equal(0, resumen.Vigentes);
        Assert.Equal(0, resumen.PorVencer);
        Assert.Equal(0, resumen.Vencidos);
        Assert.Empty(resumen.Proximos);
        Assert.Equal(Hoy, resumen.Hoy);
    }

    [Fact]
    public void Cuenta_cada_fila_en_su_balde_correcto()
    {
        var filas = new[]
        {
            Fila("2026-07-01"),   // vencido hace rato
            Fila("2026-08-25"),   // por vencer (4 dias, dentro del aviso de 30)
            Fila("2026-12-01"),   // vigente
        };

        var resumen = ArmadorDeDashboard.Armar(filas, Hoy);

        Assert.Equal(3, resumen.TotalActivos);
        Assert.Equal(1, resumen.Vencidos);
        Assert.Equal(1, resumen.PorVencer);
        Assert.Equal(1, resumen.Vigentes);
    }

    [Fact]
    public void Ordena_los_proximos_por_fecha_ascendente()
    {
        var filas = new[]
        {
            Fila("2026-12-01", "El de diciembre"),
            Fila("2026-07-01", "El vencido"),
            Fila("2026-08-25", "El de agosto"),
        };

        var resumen = ArmadorDeDashboard.Armar(filas, Hoy);

        Assert.Equal(["El vencido", "El de agosto", "El de diciembre"], resumen.Proximos.Select(p => p.Nombre));
    }

    [Fact]
    public void Respeta_el_limite_de_proximos_pero_no_el_conteo_total()
    {
        var filas = Enumerable.Range(1, 12)
            .Select(i => Fila(Hoy.AddDays(i).ToString("yyyy-MM-dd"), $"Item {i}"))
            .ToArray();

        var resumen = ArmadorDeDashboard.Armar(filas, Hoy, maxProximos: 5);

        Assert.Equal(12, resumen.TotalActivos);   // el contador no se recorta
        Assert.Equal(5, resumen.Proximos.Count);  // la lista si
    }

    [Fact]
    public void El_vencimiento_pisa_la_anticipacion_de_su_categoria_al_armar_el_estado()
    {
        // A 10 dias: con el default de la categoria (30) seria "por vencer",
        // pero el vencimiento pisa con 5 y todavia no entra en el aviso.
        var fila = Fila(Hoy.AddDays(10).ToString("yyyy-MM-dd"), diasAvisoVenc: 5, diasAvisoCat: 30);

        var resumen = ArmadorDeDashboard.Armar([fila], Hoy);

        Assert.Equal(1, resumen.Vigentes);
        Assert.Equal(0, resumen.PorVencer);
    }
}
