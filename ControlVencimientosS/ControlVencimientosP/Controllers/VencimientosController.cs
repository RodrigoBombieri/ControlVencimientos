using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ControlVencimientosP.Controllers;

public class VencimientosController : Controller
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenant;

    public VencimientosController(AppDbContext db, ITenantProvider tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpGet]
    public async Task<IActionResult> Nuevo()
    {
        var model = new NuevoVencimientoViewModel
        {
            FechaVencimiento = (await HoyAsync()).ToString("yyyy-MM-dd"),
            Categorias = await CategoriasParaSelectAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Nuevo(NuevoVencimientoViewModel model)
    {
        DateOnly fechaVencimiento = default;
        if (string.IsNullOrWhiteSpace(model.FechaVencimiento) ||
            !DateOnly.TryParseExact(model.FechaVencimiento, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaVencimiento))
        {
            ModelState.AddModelError(nameof(model.FechaVencimiento), "La fecha no es válida.");
        }

        var categoriaValida = await _db.Categorias.AnyAsync(c => c.Id == model.CategoriaId && c.Activa);
        if (!categoriaValida)
        {
            ModelState.AddModelError(nameof(model.CategoriaId), "Elegí una categoría válida.");
        }
        
        if (!ModelState.IsValid)
        {
            model.Categorias = await CategoriasParaSelectAsync();
            return View(model);
        }

        var ahora = DateTime.UtcNow;
        var item = new Item
        {
            CategoriaId = model.CategoriaId,
            Nombre = model.Nombre.Trim(),
            Codigo = string.IsNullOrWhiteSpace(model.Codigo) ? null : model.Codigo.Trim(),
            Ubicacion = string.IsNullOrWhiteSpace(model.Ubicacion) ? null : model.Ubicacion.Trim(),
            Proveedor = string.IsNullOrWhiteSpace(model.Proveedor) ? null : model.Proveedor.Trim(),
            CreadoEn = ahora
        };

        var vencimiento = new Vencimiento
        {
            FechaVencimiento = fechaVencimiento,
            Monto = model.Monto,
            CreadoPorUsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            CreadoEn = ahora,
            Item = item
        };

        // Un solo Add alcanza: EF sigue la navegación Vencimiento -> Item e
        // inserta las dos filas. AppDbContext.SaveChangesAsync se encarga de
        // pisar el EmpresaId de ambas antes de guardar.
        _db.Vencimientos.Add(vencimiento);
        await _db.SaveChangesAsync();

        TempData["Mensaje"] = $"«{item.Nombre}» se agregó correctamente.";
        return RedirectToAction("Index", "Home");
    }

    private async Task<DateOnly> HoyAsync()
    {
        var empresa = await _db.Empresas.FindAsync(_tenant.EmpresaId);
        var zonaHoraria = empresa?.ZonaHoraria ?? "America/Argentina/Buenos_Aires";
        return CalculadoraDeEstado.HoyEnLaEmpresa(zonaHoraria, DateTimeOffset.UtcNow);
    }

    private async Task<List<SelectListItem>> CategoriasParaSelectAsync()
        => await _db.Categorias
            .Where(c => c.Activa)
            .OrderBy(c => c.Orden)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Nombre })
            .ToListAsync();
}
