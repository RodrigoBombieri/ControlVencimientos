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

            var resumen = ArmadorDeDashboard.Armar(filas, hoy);

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
