using ControlVencimientosP.Domain;
using ControlVencimientosP.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ControlVencimientosP.Controllers
{
    public class CuentaController : Controller
    {
        private readonly SignInManager<Usuario> _signInManager;
        private readonly ILogger<CuentaController> _logger; // Para registrar eventos de inicio de sesión y errores

        public CuentaController(SignInManager<Usuario> signInManager, ILogger<CuentaController> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Si el usuario ya está logueado, redirigirlo a la página de inicio
            // Si no está logueado, mostrar la vista de login
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]// Protege contra ataques CSRF
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Intentar iniciar sesión con el email y la contraseña proporcionados
            var resultado = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
            // Registrar el resultado del intento de inicio de sesión
            if (resultado.Succeeded)
            {
                _logger.LogInformation("Usuario {Email} logueado correctamente.", model.Email);
                return RedirectDespuesDeLogin(model.ReturnUrl);
            }

            if (resultado.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Cuenta bloqueada temporalmente por varios intentos fallidos.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Usuario deslogueado.");
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult SinAcceso() => View(); // Vista para mostrar cuando el usuario no tiene acceso a una página

        private IActionResult RedirectDespuesDeLogin(string? returnUrl)
        {
            // Url.IsLocalUrl es la parte que importa: sin ella, alguien podría
            // mandarte un link "Login?returnUrl=https://sitio-malicioso.com" y
            // terminarías redirigiendo ahí despues de un login legitimo.
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }
    }
}
