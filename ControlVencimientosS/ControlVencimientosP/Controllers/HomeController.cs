using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlVencimientosP.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ITenantProvider _tenant;

        public HomeController(AppDbContext db, ITenantProvider tenant)
        {
            _db = db;
            _tenant = tenant;
        }

        public async Task<IActionResult> Index()
        {
            var empresa = await _db.Empresas.FindAsync(_tenant.EmpresaId);
            var zonaHoraria = empresa?.ZonaHoraria ?? "America/Argentina/Buenos_Aires";
            var ahora = DateTimeOffset.UtcNow;
            var hoy = CalculadoraDeEstado.HoyEnLaEmpresa(zonaHoraria, ahora);
            var horaActual = CalculadoraDeEstado.HoraActualEnLaEmpresa(zonaHoraria, ahora);

            // El query filter global de AppDbContext ya restringe esto a la
            // empresa actual: no hace falta filtrar por EmpresaId a mano aca.
            var filas = await _db.Vencimientos
                .Activos()
                .Select(v => new FilaVencimiento(
                    v.Id,
                    v.ItemId,
                    v.Item!.Nombre,
                    v.Item.Codigo,
                    v.Item.Categoria!.Nombre,
                    v.Item.Categoria.Icono,
                    v.FechaVencimiento,
                    v.DiasAviso,
                    v.Item.Categoria.DiasAvisoDefault))
                .ToListAsync();

            // Vencimientos que ya fueron reemplazados por una renovacion: para
            // saber CUANDO se renovaron hay que mirar el CreadoEn del que lo
            // reemplazo, no el propio (que sigue teniendo su fecha de vencimiento
            // original). No usa .Activos(): son justo los que ya no lo estan.
            var fechasDeRenovacion = await _db.Vencimientos
                .Where(v => v.Estado == EstadoVencimiento.Renovado && v.RenovadoPorVencimientoId != null)
                .Select(v => v.RenovadoPor!.CreadoEn)
                .ToListAsync();

            var resumen = ArmadorDeDashboard.Armar(filas, fechasDeRenovacion, hoy);

            var config = await _db.ConfiguracionesAvisos.FindAsync(_tenant.EmpresaId);
            var destinatarios = await _db.Destinatarios.CountAsync(d => d.Activo);

            resumen.ProximoAviso = ArmadorDeAvisos.Armar(
                filas,
                hoy,
                horaActual,
                config?.HoraEnvio ?? new TimeOnly(8, 0),
                config?.Hitos() ?? [],
                config?.ReenviarVencidosCadaDias ?? 0,
                destinatarios);

            return View(resumen);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
