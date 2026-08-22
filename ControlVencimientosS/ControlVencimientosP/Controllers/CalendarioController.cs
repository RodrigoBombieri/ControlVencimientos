using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlVencimientosP.Controllers;

public class CalendarioController : Controller
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenant;

    public CalendarioController(AppDbContext db, ITenantProvider tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? anio, int? mes)
    {
        var hoy = await HoyAsync();

        // Sin parametros (o con algo invalido en la URL), el mes actual.
        // Los links de navegacion (< / >) siempre mandan los dos juntos.
        var mesVisible = anio is >= 1 and <= 9999 && mes is >= 1 and <= 12
            ? new DateOnly(anio.Value, mes.Value, 1)
            : new DateOnly(hoy.Year, hoy.Month, 1);

        // El query filter global de AppDbContext ya restringe esto a la
        // empresa actual. Se trae TODO lo activo, sin acotar por rango de
        // fechas: mismo criterio que el dashboard y el listado — para una
        // pyme no vale la pena la complejidad extra de filtrar por mes en
        // la consulta.
        var filas = await _db.Vencimientos
            .Activos()
            .Select(v => new FilaCalendario(
                v.Id,
                v.ItemId,
                v.Item!.Nombre,
                v.FechaVencimiento,
                v.DiasAviso,
                v.Item.Categoria!.DiasAvisoDefault))
            .ToListAsync();

        var model = ArmadorDeCalendario.Armar(filas, mesVisible.Year, mesVisible.Month, hoy);
        return View(model);
    }

    private async Task<DateOnly> HoyAsync()
    {
        var empresa = await _db.Empresas.FindAsync(_tenant.EmpresaId);
        var zonaHoraria = empresa?.ZonaHoraria ?? "America/Argentina/Buenos_Aires";
        return CalculadoraDeEstado.HoyEnLaEmpresa(zonaHoraria, DateTimeOffset.UtcNow);
    }
}
