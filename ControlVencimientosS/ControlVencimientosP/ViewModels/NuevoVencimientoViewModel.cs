using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControlVencimientosP.ViewModels;

/// <summary>
/// Datos del formulario de alta rápida: un Item nuevo y su primer
/// Vencimiento, en una sola pantalla. "5 campos, cinco segundos" - el resto
/// queda atrás de "Más opciones".
/// </summary>
public class NuevoVencimientoViewModel
{
    // No lleva [Required]: un int en 0 no dispara esa validación. La
    // categoría real se valida en el controller contra la base (existe y
    // está activa), que es el chequeo que importa de verdad.
    [Display(Name = "Categoría")]
    public int CategoriaId { get; set; }

    [Required(ErrorMessage = "Ingresá un nombre.")]
    [StringLength(200, ErrorMessage = "Máximo 200 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    [Display(Name = "Código")]
    public string? Codigo { get; set; }

    // Viene de un <input type="date">, siempre en formato yyyy-MM-dd. Se
    // parsea a mano en el controller (DateOnly.TryParseExact) en vez de
    // depender del model binder por defecto para ese formato.
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

    // Se completa en el controller para pintar el <select>; no se postea
    // desde el formulario.
    public IEnumerable<SelectListItem> Categorias { get; set; } = [];
}
