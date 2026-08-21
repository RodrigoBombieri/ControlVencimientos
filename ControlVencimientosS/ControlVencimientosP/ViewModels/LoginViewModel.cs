using System.ComponentModel.DataAnnotations;

namespace ControlVencimientosP.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Ingresá tu email.")]
        [EmailAddress(ErrorMessage = "El email no es válido.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresá tu contraseña.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Recordarme")]
        public bool RememberMe { get; set; }
        // Para redirigir al usuario a la página que estaba intentando acceder antes de loguearse
        public string? ReturnUrl { get; set; }
    }
}
