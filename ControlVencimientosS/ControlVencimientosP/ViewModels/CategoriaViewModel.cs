using System.ComponentModel.DataAnnotations;

namespace ControlVencimientosP.ViewModels;

/// <summary>Alta y edición de una categoría.</summary>
public class CategoriaViewModel
{
    [Required(ErrorMessage = "Ingresá un nombre.")]
    [StringLength(80, ErrorMessage = "Máximo 80 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Range(0, 365, ErrorMessage = "Tiene que estar entre 0 y 365 días.")]
    [Display(Name = "Aviso (días antes)")]
    public int DiasAvisoDefault { get; set; } = 30;

    /// <summary>Uno de <see cref="ArmadorDeConfiguracion.IconosDisponibles"/>. Se valida y normaliza en el controller.</summary>
    public string Icono { get; set; } = ArmadorDeConfiguracion.IconoPorDefecto;
}
