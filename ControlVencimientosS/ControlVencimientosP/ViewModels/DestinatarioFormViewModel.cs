using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControlVencimientosP.ViewModels;

/// <summary>Alta y edición de un destinatario de avisos.</summary>
public class DestinatarioFormViewModel
{
    [Required(ErrorMessage = "Ingresá un nombre.")]
    [StringLength(200, ErrorMessage = "Máximo 200 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Máximo 200 caracteres.")]
    [EmailAddress(ErrorMessage = "El email no es válido.")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    // Ids de categoría marcadas en el checklist. Vacío = recibe todas.
    public List<int> CategoriasSeleccionadas { get; set; } = [];

    // Se completa en el controller para pintar el checklist; no se postea
    // desde el formulario.
    public IReadOnlyList<SelectListItem> CategoriasDisponibles { get; set; } = [];
}
