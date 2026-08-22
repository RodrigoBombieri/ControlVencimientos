namespace ControlVencimientosP.Services;

/// <summary>Una fila cruda para el motor de avisos: lo minimo para decidir y despues armar el mail.</summary>
public record FilaVencimientoAviso(
    int VencimientoId,
    string ItemNombre,
    string? ItemCodigo,
    int CategoriaId,
    DateOnly FechaVencimiento);

/// <summary>
/// Identifica un aviso ya mandado. Con esto (mas el indice unico de la base)
/// el motor nunca manda el mismo mail dos veces.
/// </summary>
public record AvisoClave(int VencimientoId, int HitoDias, CanalAviso Canal, int DestinatarioId);

/// <summary>Un item que corresponde avisar hoy, ya resuelto contra los hitos.</summary>
public record ItemAEnviar(int VencimientoId, string ItemNombre, string? ItemCodigo, int DiasRestantes, bool EsUrgente);

/// <summary>Un mail listo para armar y mandar: un destinatario, un tipo, sus items.</summary>
public record EnvioPlanificado(
    int DestinatarioId,
    string Email,
    bool EsAlertaIndividual,
    IReadOnlyList<ItemAEnviar> Items);

/// <summary>
/// Decide, por destinatario, que vencimientos le corresponde avisar hoy y en
/// que mail (digest o alerta individual). Funcion pura a proposito, como el
/// resto del motor: la idempotencia (que ya se mando) y el filtro de
/// categorias entran como datos ya resueltos, no se consultan aca. El job de
/// Hangfire es el unico que sabe de AppDbContext y de SMTP.
/// </summary>
public static class ArmadorDeEnvios
{
    public static IReadOnlyList<EnvioPlanificado> Armar(
        IEnumerable<FilaVencimientoAviso> filas,
        IEnumerable<Destinatario> destinatarios,
        DateOnly hoy,
        IReadOnlyCollection<int> hitos,
        int reenviarVencidosCadaDias,
        bool digestActivo,
        bool alertaIndividualActiva,
        IReadOnlySet<AvisoClave> yaEnviados)
    {
        var filasList = filas as IReadOnlyList<FilaVencimientoAviso> ?? filas.ToList();
        var envios = new List<EnvioPlanificado>();

        foreach (var destinatario in destinatarios)
        {
            if (string.IsNullOrWhiteSpace(destinatario.Email))
                continue;

            var pendientes = filasList
                .Where(f => destinatario.CubreCategoria(f.CategoriaId))
                .Select(f => (Fila: f, Dias: CalculadoraDeEstado.DiasRestantes(f.FechaVencimiento, hoy)))
                .Where(x => ReglaDeAvisos.CorrespondeAvisar(x.Dias, hitos, reenviarVencidosCadaDias))
                .Where(x => !yaEnviados.Contains(new AvisoClave(x.Fila.VencimientoId, x.Dias, CanalAviso.Email, destinatario.Id)))
                .Select(x => new ItemAEnviar(x.Fila.VencimientoId, x.Fila.ItemNombre, x.Fila.ItemCodigo, x.Dias, ReglaDeAvisos.EsUrgente(x.Dias)))
                .ToList();

            if (pendientes.Count == 0)
                continue;

            var urgentes = pendientes.Where(i => i.EsUrgente).ToList();
            var resto = pendientes.Where(i => !i.EsUrgente).ToList();

            if (alertaIndividualActiva && urgentes.Count > 0)
                envios.Add(new EnvioPlanificado(destinatario.Id, destinatario.Email!, true, urgentes));

            // Si la alerta individual esta apagada, lo urgente cae en el
            // digest en vez de perderse: que un canal este desactivado no
            // puede hacer que un vencimiento urgente no se avise nunca.
            var paraDigest = alertaIndividualActiva ? resto : pendientes;
            if (digestActivo && paraDigest.Count > 0)
                envios.Add(new EnvioPlanificado(destinatario.Id, destinatario.Email!, false, paraDigest));
        }

        return envios;
    }
}
