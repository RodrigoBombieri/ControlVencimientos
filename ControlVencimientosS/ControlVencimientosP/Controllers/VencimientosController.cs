using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ControlVencimientosP.Controllers;

public class VencimientosController : Controller
{
    // PDF, JPG o PNG hasta 10 MB. Alcanza y sobra para el certificado de un
    // tramite; ampliar el dia que haga falta otra cosa.
    private static readonly string[] ExtensionesAdjuntoPermitidas = [".pdf", ".jpg", ".jpeg", ".png"];
    private const long TamanioMaximoAdjuntoBytes = 10 * 1024 * 1024;

    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenant;
    private readonly IWebHostEnvironment _entorno;

    public VencimientosController(AppDbContext db, ITenantProvider tenant, IWebHostEnvironment entorno)
    {
        _db = db;
        _tenant = tenant;
        _entorno = entorno;
    }

    // Los adjuntos viven afuera de wwwroot a proposito: si estuvieran ahi
    // serian servibles por URL directa sin pasar por autenticacion ni por
    // el filtro de empresa. Se sirven siempre a traves de DescargarAdjunto.
    private string CarpetaAdjuntos => Path.Combine(_entorno.ContentRootPath, "App_Data", "adjuntos");

    [HttpGet]
    public async Task<IActionResult> Index(string? estado, int? categoriaId, string? q)
    {
        var hoy = await HoyAsync();

        // El query filter global de AppDbContext ya restringe esto a la
        // empresa actual: no hace falta filtrar por EmpresaId a mano aca.
        var filas = await _db.Vencimientos
            .Activos()
            .Select(v => new FilaListado(
                v.Id,
                v.ItemId,
                v.Item!.Nombre,
                v.Item.Codigo,
                v.Item.Ubicacion,
                v.Item.CategoriaId,
                v.Item.Categoria!.Nombre,
                v.Item.Categoria.Icono,
                v.FechaVencimiento,
                v.DiasAviso,
                v.Item.Categoria.DiasAvisoDefault))
            .ToListAsync();

        var filtroEstado = ParsearEstado(estado);

        // Se calcula el semaforo una sola vez y se filtra dos veces sobre el
        // mismo resultado: una vez sin el filtro de estado (para los
        // totales del subtitulo) y otra con todos los filtros (para la
        // tabla). Asi el subtitulo no cambia segun que chip este activo.
        var todos = ArmadorDeListado.Calcular(filas, hoy);
        var filtrados = ArmadorDeListado.Filtrar(todos, filtroEstado, categoriaId, q);

        var model = new ListadoVencimientosViewModel
        {
            Items = filtrados,
            Total = todos.Count,
            Vencidos = todos.Count(i => i.Estado == EstadoSemaforo.Vencido),
            PorVencer = todos.Count(i => i.Estado == EstadoSemaforo.PorVencer),
            FiltroEstado = filtroEstado,
            FiltroCategoriaId = categoriaId,
            FiltroTexto = q,
            Categorias = await CategoriasParaSelectAsync()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var vencimiento = await _db.Vencimientos
            .Include(v => v.Item!).ThenInclude(i => i!.Categoria)
            .Include(v => v.Adjuntos)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vencimiento is null)
        {
            return NotFound();
        }

        var hoy = await HoyAsync();
        var diasAviso = CalculadoraDeEstado.DiasAvisoEfectivo(vencimiento.DiasAviso, vencimiento.Item!.Categoria!.DiasAvisoDefault);

        var model = new DetalleVencimientoViewModel
        {
            VencimientoId = vencimiento.Id,
            ItemId = vencimiento.ItemId,
            ItemNombre = vencimiento.Item.Nombre,
            ItemCodigo = vencimiento.Item.Codigo,
            ItemUbicacion = vencimiento.Item.Ubicacion,
            ItemProveedor = vencimiento.Item.Proveedor,
            CategoriaNombre = vencimiento.Item.Categoria.Nombre,
            FechaEmision = vencimiento.FechaEmision,
            FechaVencimiento = vencimiento.FechaVencimiento,
            NumeroDocumento = vencimiento.NumeroDocumento,
            Monto = vencimiento.Monto,
            Moneda = vencimiento.Moneda,
            Estado = vencimiento.Estado,
            EstadoSemaforo = CalculadoraDeEstado.Calcular(vencimiento.FechaVencimiento, diasAviso, hoy),
            DiasRestantes = CalculadoraDeEstado.DiasRestantes(vencimiento.FechaVencimiento, hoy),
            CreadoEn = vencimiento.CreadoEn,
            RenovadoPorVencimientoId = vencimiento.RenovadoPorVencimientoId,
            Adjuntos = vencimiento.Adjuntos.OrderByDescending(a => a.SubidoEn).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Renovar(int id)
    {
        var anterior = await _db.Vencimientos
            .Include(v => v.Item!).ThenInclude(i => i!.Categoria)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (anterior is null)
        {
            return NotFound();
        }

        // Solo tiene sentido renovar el vigente de un item: uno que ya esta
        // anulado o ya fue renovado no admite "renovarse" de nuevo.
        if (anterior.Estado != EstadoVencimiento.Activo)
        {
            return RedirectToAction(nameof(Detalle), new { id });
        }

        var model = new RenovarVencimientoViewModel
        {
            VencimientoId = anterior.Id,
            ItemNombre = anterior.Item!.Nombre,
            ItemCodigo = anterior.Item.Codigo,
            CategoriaNombre = anterior.Item.Categoria!.Nombre,
            FechaVencimientoAnterior = anterior.FechaVencimiento,
            FechaVencimiento = (await HoyAsync()).ToString("yyyy-MM-dd"),
            Monto = anterior.Monto
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Renovar(int id, RenovarVencimientoViewModel model)
    {
        var anterior = await _db.Vencimientos
            .Include(v => v.Item!).ThenInclude(i => i!.Categoria)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (anterior is null)
        {
            return NotFound();
        }

        if (anterior.Estado != EstadoVencimiento.Activo)
        {
            return RedirectToAction(nameof(Detalle), new { id });
        }

        DateOnly fechaVencimiento = default;
        if (string.IsNullOrWhiteSpace(model.FechaVencimiento) ||
            !DateOnly.TryParseExact(model.FechaVencimiento, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaVencimiento))
        {
            ModelState.AddModelError(nameof(model.FechaVencimiento), "La fecha no es válida.");
        }

        if (!ModelState.IsValid)
        {
            model.VencimientoId = anterior.Id;
            model.ItemNombre = anterior.Item!.Nombre;
            model.ItemCodigo = anterior.Item.Codigo;
            model.CategoriaNombre = anterior.Item.Categoria!.Nombre;
            model.FechaVencimientoAnterior = anterior.FechaVencimiento;
            return View(model);
        }

        // Dos SaveChanges a proposito, en ese orden: primero se marca el
        // anterior como Renovado y se guarda; recien despues se inserta el
        // nuevo Activo. Si se hiciera todo junto en un solo SaveChanges, el
        // orden en que EF ejecuta los statements no esta garantizado, y se
        // puede terminar insertando el nuevo vencimiento Activo mientras el
        // anterior todavia sigue Activo en la base — lo que viola
        // UX_Vencimientos_UnoActivoPorItem (un solo activo por item).
        anterior.Estado = EstadoVencimiento.Renovado;
        await _db.SaveChangesAsync();

        var nuevo = new Vencimiento
        {
            ItemId = anterior.ItemId,
            FechaVencimiento = fechaVencimiento,
            Monto = model.Monto,
            CreadoPorUsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            CreadoEn = DateTime.UtcNow
        };

        // Se linkea por navegacion, no por el Id escalar: el Id de "nuevo"
        // todavia no existe en este punto (lo genera el INSERT), y seteando
        // la navegacion EF arma la relacion sola despues de insertarlo, en
        // el mismo SaveChanges.
        anterior.RenovadoPor = nuevo;
        _db.Vencimientos.Add(nuevo);
        await _db.SaveChangesAsync();

        TempData["Mensaje"] = $"«{anterior.Item!.Nombre}» se renovó correctamente.";
        return RedirectToAction(nameof(Detalle), new { id = nuevo.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var vencimiento = await _db.Vencimientos.Include(v => v.Item).FirstOrDefaultAsync(v => v.Id == id);
        if (vencimiento is null)
        {
            return NotFound();
        }

        // No tiene sentido editar algo que ya fue anulado o renovado: eso
        // es historial. Se manda de vuelta al detalle en vez de mostrar un
        // formulario que no va a poder guardar.
        if (vencimiento.Estado != EstadoVencimiento.Activo)
        {
            return RedirectToAction(nameof(Detalle), new { id });
        }

        var model = new EditarVencimientoViewModel
        {
            VencimientoId = vencimiento.Id,
            CategoriaId = vencimiento.Item!.CategoriaId,
            Nombre = vencimiento.Item.Nombre,
            Codigo = vencimiento.Item.Codigo,
            FechaVencimiento = vencimiento.FechaVencimiento.ToString("yyyy-MM-dd"),
            Ubicacion = vencimiento.Item.Ubicacion,
            Proveedor = vencimiento.Item.Proveedor,
            Monto = vencimiento.Monto,
            Categorias = await CategoriasParaSelectAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, EditarVencimientoViewModel model)
    {
        var vencimiento = await _db.Vencimientos.Include(v => v.Item).FirstOrDefaultAsync(v => v.Id == id);
        if (vencimiento is null)
        {
            return NotFound();
        }

        if (vencimiento.Estado != EstadoVencimiento.Activo)
        {
            return RedirectToAction(nameof(Detalle), new { id });
        }

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
            model.VencimientoId = id;
            model.Categorias = await CategoriasParaSelectAsync();
            return View(model);
        }

        // A diferencia del alta, esto no crea filas nuevas: corrige las
        // que ya existen. EF las esta siguiendo (vienen de un Include),
        // asi que alcanza con cambiar las propiedades y guardar.
        vencimiento.Item!.CategoriaId = model.CategoriaId;
        vencimiento.Item.Nombre = model.Nombre.Trim();
        vencimiento.Item.Codigo = string.IsNullOrWhiteSpace(model.Codigo) ? null : model.Codigo.Trim();
        vencimiento.Item.Ubicacion = string.IsNullOrWhiteSpace(model.Ubicacion) ? null : model.Ubicacion.Trim();
        vencimiento.Item.Proveedor = string.IsNullOrWhiteSpace(model.Proveedor) ? null : model.Proveedor.Trim();
        vencimiento.FechaVencimiento = fechaVencimiento;
        vencimiento.Monto = model.Monto;

        await _db.SaveChangesAsync();

        TempData["Mensaje"] = $"«{vencimiento.Item.Nombre}» se actualizó correctamente.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Anular(int id)
    {
        var vencimiento = await _db.Vencimientos.Include(v => v.Item).FirstOrDefaultAsync(v => v.Id == id);
        if (vencimiento is null)
        {
            return NotFound();
        }

        // Idempotente a proposito: si por doble click o back-forward llega
        // dos veces, la segunda no rompe nada, simplemente no hace nada.
        if (vencimiento.Estado == EstadoVencimiento.Activo)
        {
            vencimiento.Estado = EstadoVencimiento.Anulado;
            await _db.SaveChangesAsync();
            TempData["Mensaje"] = $"«{vencimiento.Item?.Nombre}» se anuló. El ítem queda libre para cargar un vencimiento nuevo.";
        }

        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubirAdjunto(int id, IFormFile? archivo)
    {
        var vencimiento = await _db.Vencimientos.FirstOrDefaultAsync(v => v.Id == id);
        if (vencimiento is null)
        {
            return NotFound();
        }

        if (archivo is null || archivo.Length == 0)
        {
            TempData["Error"] = "Elegí un archivo para subir.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!ExtensionesAdjuntoPermitidas.Contains(extension))
        {
            TempData["Error"] = "Solo se aceptan archivos PDF, JPG o PNG.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        if (archivo.Length > TamanioMaximoAdjuntoBytes)
        {
            TempData["Error"] = "El archivo no puede superar los 10 MB.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        // Una subcarpeta por empresa y por vencimiento: ademas de prolijo,
        // hace que borrar un vencimiento (cascade) no deje archivos sueltos
        // dificiles de rastrear.
        var rutaRelativa = Path.Combine(_tenant.EmpresaId.ToString(), id.ToString(), $"{Guid.NewGuid()}{extension}");
        var rutaCompleta = Path.Combine(CarpetaAdjuntos, rutaRelativa);
        Directory.CreateDirectory(Path.GetDirectoryName(rutaCompleta)!);

        using (var destino = System.IO.File.Create(rutaCompleta))
        {
            await archivo.CopyToAsync(destino);
        }

        _db.Adjuntos.Add(new Adjunto
        {
            VencimientoId = id,
            NombreArchivo = archivo.FileName,
            RutaBlob = rutaRelativa,
            ContentType = archivo.ContentType,
            TamanioBytes = archivo.Length,
            SubidoEn = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        TempData["Mensaje"] = "Adjunto subido correctamente.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> DescargarAdjunto(int id)
    {
        // El query filter global ya restringe esto a la empresa actual:
        // pedir el adjunto de otra empresa por id simplemente no aparece.
        var adjunto = await _db.Adjuntos.FirstOrDefaultAsync(a => a.Id == id);
        if (adjunto is null)
        {
            return NotFound();
        }

        var rutaCompleta = Path.Combine(CarpetaAdjuntos, adjunto.RutaBlob);
        if (!System.IO.File.Exists(rutaCompleta))
        {
            return NotFound();
        }

        var contentType = string.IsNullOrWhiteSpace(adjunto.ContentType) ? "application/octet-stream" : adjunto.ContentType;
        return PhysicalFile(rutaCompleta, contentType, adjunto.NombreArchivo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarAdjunto(int id)
    {
        var adjunto = await _db.Adjuntos.FirstOrDefaultAsync(a => a.Id == id);
        if (adjunto is null)
        {
            return NotFound();
        }

        var vencimientoId = adjunto.VencimientoId;
        var rutaCompleta = Path.Combine(CarpetaAdjuntos, adjunto.RutaBlob);

        _db.Adjuntos.Remove(adjunto);
        await _db.SaveChangesAsync();

        // El borrado logico (la fila) es lo que importa; si el archivo
        // fisico ya no esta o falla el borrado, no vale la pena bloquear
        // al usuario por eso.
        try
        {
            System.IO.File.Delete(rutaCompleta);
        }
        catch (IOException)
        {
        }

        TempData["Mensaje"] = "Adjunto eliminado.";
        return RedirectToAction(nameof(Detalle), new { id = vencimientoId });
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

    private static EstadoSemaforo? ParsearEstado(string? estado) => estado?.ToLowerInvariant() switch
    {
        "vigente" => EstadoSemaforo.Vigente,
        "porvencer" => EstadoSemaforo.PorVencer,
        "vencido" => EstadoSemaforo.Vencido,
        _ => null
    };

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
