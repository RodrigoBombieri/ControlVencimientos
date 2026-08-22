using System.ComponentModel.DataAnnotations;

namespace ControlVencimientosP.ViewModels;

/// <summary>
/// Datos para renovar un vencimiento: crea uno nuevo y deja el actual como
/// historial (a diferencia de Editar, que corrige el mismo registro). Solo
/// pide lo mismo que pide "Nuevo" del lado del vencimiento en si (fecha y
/// monto) — el ítem (nombre, código, ubicación, proveedor) no cambia, así
/// que ni se muestra como campo editable acá.
/// </summary>
public class RenovarVencimientoViewModel
{
    public int VencimientoId { get; set; }

    // Solo para mostrar contexto en la pantalla; no se postea.
    public string ItemNombre { get; set; } = string.Empty;
    public string? ItemCodigo { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public DateOnly FechaVencimientoAnterior { get; set; }

    [Required(ErrorMessage = "Ingresá la fecha de vencimiento.")]
    [Display(Name = "Vence el")]
    public string FechaVencimiento { get; set; } = string.Empty;

    [Range(0, 999999999, ErrorMessage = "El monto no puede ser negativo.")]
    [Display(Name = "Monto")]
    public decimal? Monto { get; set; }
}
