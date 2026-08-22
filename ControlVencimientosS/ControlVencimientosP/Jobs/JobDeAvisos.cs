using Microsoft.EntityFrameworkCore;

namespace ControlVencimientosP.Jobs;

/// <summary>
/// El motor de avisos automaticos. Hangfire lo dispara todas las horas en
/// punto; por cada empresa activa, si es su hora de envio configurada, decide
/// que vencimientos avisar y manda los mails.
///
/// Es la unica pieza sin tests dedicados de todo el motor: junta las tres
/// cosas que <see cref="Services.ArmadorDeEnvios"/> y
/// <see cref="Services.ArmadorDeMailDeAviso"/> evitan a proposito
/// (AppDbContext, SMTP y el reloj) para poder testear la logica sin ellas.
///
/// Corre fuera de cualquier request HTTP, asi que no puede usar el
/// <c>AppDbContext</c> ni el <c>ITenantProvider</c> que resuelve el
/// contenedor de DI (ese depende de <c>HttpContext</c> y cae siempre a la
/// empresa 1). En cambio arma un <c>AppDbContext</c> por empresa a mano, con
/// <see cref="Tenancy.TenantProviderFijo"/>, para que el query filter global
/// quede scopeado a la empresa correcta.
/// </summary>
public class JobDeAvisos(
    DbContextOptions<AppDbContext> opcionesDb,
    IEnviadorDeMail enviador,
    ILogger<JobDeAvisos> logger)
{
    public async Task EjecutarAsync(CancellationToken cancelacion = default)
    {
        // Empresa no implementa IEntidadDeEmpresa (ella ES el tenant): este
        // contexto ve todas las empresas sin importar el EmpresaId con el que
        // se construyo el TenantProviderFijo.
        await using var dbEmpresas = new AppDbContext(opcionesDb, new TenantProviderFijo(0));
        var empresas = await dbEmpresas.Empresas.Where(e => e.Activa).ToListAsync(cancelacion);

        foreach (var empresa in empresas)
        {
            try
            {
                await ProcesarEmpresaAsync(empresa, cancelacion);
            }
            catch (Exception ex)
            {
                // Una empresa que falla (SMTP caido, config invalida) no
                // puede frenar el aviso de las demas.
                logger.LogError(ex, "Fallo el motor de avisos para la empresa {EmpresaId}", empresa.Id);
            }
        }
    }

    private async Task ProcesarEmpresaAsync(Empresa empresa, CancellationToken cancelacion)
    {
        await using var db = new AppDbContext(opcionesDb, new TenantProviderFijo(empresa.Id));

        var ahora = DateTimeOffset.UtcNow;
        var horaActual = CalculadoraDeEstado.HoraActualEnLaEmpresa(empresa.ZonaHoraria, ahora);
        var hoy = CalculadoraDeEstado.HoyEnLaEmpresa(empresa.ZonaHoraria, ahora);

        var config = await db.ConfiguracionesAvisos.FindAsync([empresa.Id], cancelacion);
        if (config is null)
            return;

        // El job corre cada hora en punto; solo actua en la hora configurada
        // para esta empresa, las demas corridas del dia no hacen nada aca.
        if (horaActual.Hour != config.HoraEnvio.Hour)
            return;

        var destinatarios = await db.Destinatarios.Where(d => d.Activo).ToListAsync(cancelacion);
        if (destinatarios.Count == 0)
            return;

        var filas = await db.Vencimientos
            .Activos()
            .Select(v => new FilaVencimientoAviso(v.Id, v.Item!.Nombre, v.Item.Codigo, v.Item.CategoriaId, v.FechaVencimiento))
            .ToListAsync(cancelacion);

        if (filas.Count == 0)
            return;

        var vencimientoIds = filas.Select(f => f.VencimientoId).ToList();
        var yaEnviados = await db.AvisosEnviados
            .Where(a => vencimientoIds.Contains(a.VencimientoId) && a.Canal == CanalAviso.Email)
            .Select(a => new AvisoClave(a.VencimientoId, a.HitoDias, a.Canal, a.DestinatarioId))
            .ToListAsync(cancelacion);

        var envios = ArmadorDeEnvios.Armar(
            filas,
            destinatarios,
            hoy,
            config.Hitos(),
            config.ReenviarVencidosCadaDias,
            config.DigestActivo,
            config.AlertaIndividualActiva,
            yaEnviados.ToHashSet());

        foreach (var envio in envios)
            await EnviarYRegistrarAsync(db, envio, hoy, cancelacion);
    }

    private async Task EnviarYRegistrarAsync(
        AppDbContext db, EnvioPlanificado envio, DateOnly hoy, CancellationToken cancelacion)
    {
        var items = envio.Items
            .Select(i => new ItemParaMail(i.VencimientoId, i.ItemNombre, i.ItemCodigo, i.DiasRestantes))
            .ToList();

        var mailArmado = envio.EsAlertaIndividual
            ? ArmadorDeMailDeAviso.ArmarAlertaIndividual(items, hoy)
            : ArmadorDeMailDeAviso.ArmarDigest(items, hoy);

        try
        {
            await enviador.EnviarAsync(
                new MailParaEnviar(envio.Email, mailArmado.Asunto, mailArmado.CuerpoHtml), cancelacion);
        }
        catch (Exception ex)
        {
            // No se persiste nada: sin fila en AvisoEnviado para estos items,
            // el proximo tick reintenta solo, sin tocar el indice de
            // idempotencia.
            logger.LogWarning(ex, "Fallo el envio de mail al destinatario {DestinatarioId}", envio.DestinatarioId);
            return;
        }

        var ahoraUtc = DateTime.UtcNow;
        foreach (var item in envio.Items)
        {
            db.AvisosEnviados.Add(new AvisoEnviado
            {
                VencimientoId = item.VencimientoId,
                HitoDias = item.DiasRestantes,
                Canal = CanalAviso.Email,
                DestinatarioId = envio.DestinatarioId,
                EnviadoEn = ahoraUtc,
                Estado = EstadoAviso.Enviado
            });
        }

        await db.SaveChangesAsync(cancelacion);
    }
}
