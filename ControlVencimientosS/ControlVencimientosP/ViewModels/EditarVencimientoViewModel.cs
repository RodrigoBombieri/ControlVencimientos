using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControlVencimientosP.ViewModels;

/// <summary>
/// Mismos campos que <see cref="NuevoVencimientoViewModel"/>, pero para
/// corregir un item+vencimiento que ya existe en vez de crear uno nuevo.
/// Se mantiene como clase aparte a propósito: así el alta rápida no se ve
/// afectada por lo que necesite la edición más adelante (por ejemplo, el
/// día que se agreguen más campos editables que no tiene sentido pedir en
/// el alta).
/// </summary>
public class EditarVencimientoViewModel
{
    public int VencimientoId { get; set; }

    [Display(Name = "Categoría")]
    public int CategoriaId { get; set; }

    [Required(ErrorMessage = "Ingresá un nombre.")]
    [StringLength(200, ErrorMessage = "Máximo 200 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    [Display(Name = "Código")]
    public string? Codigo { get; set; }

    [Required(ErrorMessage = "Ingresá la fecha de vencimiento.")]
    [Display(Name = "Vence el")]
    public string FechaVencimiento { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Máximo 200 caracteres.")]
    [Display(Name = "Ubicación")]
    public string? Ubicacion { get; set; }

    [StringLength(200, ErrorMessage = "Máximo 200 caracteres.")]
    [Display(Name = "Proveedor")]
    public string? Proveedor { get; set; }

    [Range(0, 999999999, ErrorMessage = "El monto no puede ser negativo.")]
    [Display(Name = "Monto")]
    public decimal? Monto { get; set; }

    public IEnumerable<SelectListItem> Categorias { get; set; } = [];
}
