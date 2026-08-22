using System.ComponentModel.DataAnnotations;

namespace ControlVencimientosP.ViewModels;

/// <summary>Formulario de "Avisos automáticos" en Configuración.</summary>
public class AvisosConfigViewModel
{
    [Display(Name = "Resumen diario por email")]
    public bool DigestActivo { get; set; }

    [Display(Name = "Alerta individual urgente")]
    public bool AlertaIndividualActiva { get; set; }

    // Se postea como una lista de checkboxes tildados (ver ArmadorDeConfiguracion.HitosDisponibles),
    // no como texto libre: asi el usuario nunca puede escribir un hito invalido.
    // La validacion de "al menos uno" se hace a mano en el controller, no con
    // [Required]: en una lista vacia, [Required] no dispara.
    [Display(Name = "Hitos de aviso")]
    public List<int> HitosSeleccionados { get; set; } = [60, 30, 15, 7, 1, 0];

    [Range(0, 365, ErrorMessage = "Tiene que estar entre 0 y 365 días.")]
    [Display(Name = "Recordar lo vencido cada (días)")]
    public int ReenviarVencidosCadaDias { get; set; } = 7;

    // Viene de un <input type="time">, siempre en formato HH:mm. Se parsea a
    // mano en el controller, mismo criterio que FechaVencimiento en los
    // formularios de vencimientos.
    [Required(ErrorMessage = "Ingresá la hora de envío.")]
    [Display(Name = "Hora de envío")]
    public string HoraEnvio { get; set; } = "08:00";

    // Solo para mostrar: el switch queda deshabilitado en la vista, todavía
    // no se puede activar (requiere WhatsApp Business verificado). No se
    // postea desde el formulario.
    public bool WhatsappActivo { get; set; }
}
