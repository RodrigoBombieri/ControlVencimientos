using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ControlVencimientosP.Controllers;

public class ConfiguracionController : Controller
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenant;

    public ConfiguracionController(AppDbContext db, ITenantProvider tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = await ArmarViewModelAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarAvisos([Bind(Prefix = "Avisos")] AvisosConfigViewModel model)
    {
        // El formulario vive adentro de ConfiguracionViewModel (asp-for="Avisos.X"),
        // asi que postea con el prefijo "Avisos.": el [Bind(Prefix)] hace que el
        // model binder lo encuentre, y las claves de ModelState que se agregan a
        // mano abajo usan el mismo prefijo para que asp-validation-for="Avisos.X"
        // las encuentre.
        var hitos = ArmadorDeConfiguracion.ArmarHitos(model.HitosSeleccionados);
        if (!hitos.EsValido)
        {
            ModelState.AddModelError("Avisos.HitosSeleccionados", "Elegí al menos un hito.");
        }

        TimeOnly horaEnvio = default;
        if (string.IsNullOrWhiteSpace(model.HoraEnvio) ||
            !TimeOnly.TryParseExact(model.HoraEnvio, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out horaEnvio))
        {
            ModelState.AddModelError("Avisos.HoraEnvio", "La hora no es válida.");
        }

        if (!ModelState.IsValid)
        {
            var vm = await ArmarViewModelAsync();
            vm.Avisos = model;
            return View(nameof(Index), vm);
        }

        var config = await _db.ConfiguracionesAvisos.FindAsync(_tenant.EmpresaId);
        if (config is null)
        {
            // No debería pasar (SeedInicial la crea para la empresa 1), pero
            // una empresa nueva en un futuro multiempresa todavía no la
            // tendría: se crea acá en vez de romper la pantalla.
            config = new ConfiguracionAvisos { EmpresaId = _tenant.EmpresaId };
            _db.ConfiguracionesAvisos.Add(config);
        }

        config.DigestActivo = model.DigestActivo;
        config.AlertaIndividualActiva = model.AlertaIndividualActiva;
        config.HitosDias = hitos.HitosDias;
        config.ReenviarVencidosCadaDias = model.ReenviarVencidosCadaDias;
        config.HoraEnvio = horaEnvio;

        await _db.SaveChangesAsync();

        TempData["Mensaje"] = "La configuración de avisos se guardó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> NuevoDestinatario()
    {
        var model = new DestinatarioFormViewModel
        {
            CategoriasDisponibles = await CategoriasParaChecklistAsync([])
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NuevoDestinatario(DestinatarioFormViewModel model)
    {
        ValidarContacto(model);

        if (!ModelState.IsValid)
        {
            model.CategoriasDisponibles = await CategoriasParaChecklistAsync(model.CategoriasSeleccionadas);
            return View(model);
        }

        var destinatario = new Destinatario
        {
            Nombre = model.Nombre.Trim(),
            Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
            Telefono = string.IsNullOrWhiteSpace(model.Telefono) ? null : model.Telefono.Trim(),
            CategoriasFiltro = model.CategoriasSeleccionadas.Count == 0 ? null : string.Join(",", model.CategoriasSeleccionadas),
            Activo = true
        };

        _db.Destinatarios.Add(destinatario);
        await _db.SaveChangesAsync();

        TempData["Mensaje"] = $"«{destinatario.Nombre}» se agregó a los destinatarios.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> EditarDestinatario(int id)
    {
        var destinatario = await _db.Destinatarios.FirstOrDefaultAsync(d => d.Id == id);
        if (destinatario is null)
        {
            return NotFound();
        }

        var categoriasSeleccionadas = destinatario.CategoriasFiltradas();
        var model = new DestinatarioFormViewModel
        {
            Nombre = destinatario.Nombre,
            Email = destinatario.Email,
            Telefono = destinatario.Telefono,
            CategoriasSeleccionadas = categoriasSeleccionadas.ToList(),
            CategoriasDisponibles = await CategoriasParaChecklistAsync(categoriasSeleccionadas)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarDestinatario(int id, DestinatarioFormViewModel model)
    {
        var destinatario = await _db.Destinatarios.FirstOrDefaultAsync(d => d.Id == id);
        if (destinatario is null)
        {
            return NotFound();
        }

        ValidarContacto(model);

        if (!ModelState.IsValid)
        {
            model.CategoriasDisponibles = await CategoriasParaChecklistAsync(model.CategoriasSeleccionadas);
            return View(model);
        }

        destinatario.Nombre = model.Nombre.Trim();
        destinatario.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        destinatario.Telefono = string.IsNullOrWhiteSpace(model.Telefono) ? null : model.Telefono.Trim();
        destinatario.CategoriasFiltro = model.CategoriasSeleccionadas.Count == 0 ? null : string.Join(",", model.CategoriasSeleccionadas);

        await _db.SaveChangesAsync();

        TempData["Mensaje"] = $"«{destinatario.Nombre}» se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleDestinatario(int id)
    {
        var destinatario = await _db.Destinatarios.FirstOrDefaultAsync(d => d.Id == id);
        if (destinatario is null)
        {
            return NotFound();
        }

        destinatario.Activo = !destinatario.Activo;
        await _db.SaveChangesAsync();

        TempData["Mensaje"] = $"«{destinatario.Nombre}» quedó {(destinatario.Activo ? "activo" : "inactivo")}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult NuevaCategoria() => View(new CategoriaViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NuevaCategoria(CategoriaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // El siguiente Orden disponible: nueva categoría va al final de la
        // lista, no importa el orden en que se hayan creado las demás.
        var ordenMaximo = await _db.Categorias.MaxAsync(c => (int?)c.Orden) ?? 0;

        var categoria = new Categoria
        {
            Nombre = model.Nombre.Trim(),
            DiasAvisoDefault = model.DiasAvisoDefault,
            // Icono y color por defecto, iguales a "Otro" en el seed inicial:
            // esta pantalla todavía no tiene un selector de ícono/color.
            Icono = "file-text",
            Color = "#2a78d6",
            Orden = ordenMaximo + 1,
            Activa = true
        };

        _db.Categorias.Add(categoria);
        await _db.SaveChangesAsync();

        TempData["Mensaje"] = $"«{categoria.Nombre}» se agregó a las categorías.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> EditarCategoria(int id)
    {
        var categoria = await _db.Categorias.FirstOrDefaultAsync(c => c.Id == id);
        if (categoria is null)
        {
            return NotFound();
        }

        var model = new CategoriaViewModel
        {
            Nombre = categoria.Nombre,
            DiasAvisoDefault = categoria.DiasAvisoDefault
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarCategoria(int id, CategoriaViewModel model)
    {
        var categoria = await _db.Categorias.FirstOrDefaultAsync(c => c.Id == id);
        if (categoria is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        categoria.Nombre = model.Nombre.Trim();
        categoria.DiasAvisoDefault = model.DiasAvisoDefault;

        await _db.SaveChangesAsync();

        TempData["Mensaje"] = $"«{categoria.Nombre}» se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCategoria(int id)
    {
        var categoria = await _db.Categorias.FirstOrDefaultAsync(c => c.Id == id);
        if (categoria is null)
        {
            return NotFound();
        }

        categoria.Activa = !categoria.Activa;
        await _db.SaveChangesAsync();

        TempData["Mensaje"] = $"«{categoria.Nombre}» quedó {(categoria.Activa ? "activa" : "inactiva")}.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Un destinatario necesita al menos una forma de contacto para que tenga sentido.</summary>
    private void ValidarContacto(DestinatarioFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Email) && string.IsNullOrWhiteSpace(model.Telefono))
        {
            ModelState.AddModelError(nameof(model.Email), "Ingresá un email o un teléfono.");
        }
    }

    private async Task<ConfiguracionViewModel> ArmarViewModelAsync()
    {
        var config = await _db.ConfiguracionesAvisos.FindAsync(_tenant.EmpresaId);

        // Se materializan las dos tablas primero y se combinan en memoria:
        // ArmadorDeConfiguracion.ArmarDestinatarios usa Destinatario.CategoriasFiltradas(),
        // un metodo de instancia que EF no puede traducir a SQL dentro de un Select.
        var categorias = await _db.Categorias
            .OrderBy(c => c.Orden)
            .Select(c => new
            {
                c.Id,
                c.Nombre,
                c.Icono,
                c.DiasAvisoDefault,
                c.Activa,
                CantidadItems = c.Items.Count(i => i.Activo)
            })
            .ToListAsync();

        var destinatarios = await _db.Destinatarios
            .OrderBy(d => d.Nombre)
            .ToListAsync();

        var nombrePorCategoriaId = categorias.ToDictionary(c => c.Id, c => c.Nombre);

        var filasDestinatario = ArmadorDeConfiguracion.ArmarDestinatarios(
            destinatarios.Select(d => new FilaDestinatario(
                d.Id, d.Nombre, d.Email, d.Telefono, d.CategoriasFiltradas(), d.Activo)),
            nombrePorCategoriaId);

        return new ConfiguracionViewModel
        {
            Avisos = new AvisosConfigViewModel
            {
                DigestActivo = config?.DigestActivo ?? true,
                AlertaIndividualActiva = config?.AlertaIndividualActiva ?? true,
                HitosSeleccionados = (config?.Hitos() ?? [60, 30, 15, 7, 1, 0]).ToList(),
                ReenviarVencidosCadaDias = config?.ReenviarVencidosCadaDias ?? 7,
                HoraEnvio = (config?.HoraEnvio ?? new TimeOnly(8, 0)).ToString("HH:mm"),
                WhatsappActivo = config?.WhatsappActivo ?? false
            },
            Destinatarios = filasDestinatario,
            Categorias = categorias
                .Select(c => new CategoriaFila(c.Id, c.Nombre, c.Icono, c.DiasAvisoDefault, c.CantidadItems, c.Activa))
                .ToList()
        };
    }

    private async Task<List<SelectListItem>> CategoriasParaChecklistAsync(IEnumerable<int> seleccionadas)
    {
        var marcadas = seleccionadas.ToHashSet();

        var categorias = await _db.Categorias
            .Where(c => c.Activa)
            .OrderBy(c => c.Orden)
            .ToListAsync();

        return categorias
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Nombre, Selected = marcadas.Contains(c.Id) })
            .ToList();
    }
}
