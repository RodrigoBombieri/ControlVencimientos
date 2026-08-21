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
            var hoy = CalculadoraDeEstado.HoyEnLaEmpresa(zonaHoraria, DateTimeOffset.UtcNow);

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
            return View(resumen);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
