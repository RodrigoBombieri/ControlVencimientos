namespace ControlVencimientosP.Domain;

/// <summary>
/// Registro de cada aviso que sale. Es log y, sobre todo, es el mecanismo de
/// idempotencia: hay un indice UNICO sobre
/// (VencimientoId, HitoDias, Canal, DestinatarioId).
/// Sin eso, un reintento de Hangfire o un reinicio del servidor le manda el
/// mismo mail dos veces a todo el mundo.
/// </summary>
public class AvisoEnviado : IEntidadDeEmpresa
{
    public long Id { get; set; }
    public int EmpresaId { get; set; }
    public int VencimientoId { get; set; }

    /// <summary>
    /// Dias de anticipacion con los que se aviso. Negativo = ya estaba vencido
    /// (-7 es "una semana despues de vencido").
    /// </summary>
    public int HitoDias { get; set; }

    public CanalAviso Canal { get; set; }
    public int DestinatarioId { get; set; }
    public DateTime EnviadoEn { get; set; }
    public EstadoAviso Estado { get; set; }

    /// <summary>Id que devuelve el proveedor de email o WhatsApp, para rastrear.</summary>
    public string? ProveedorMessageId { get; set; }

    public string? Error { get; set; }

    public Vencimiento? Vencimiento { get; set; }
    public Destinatario? Destinatario { get; set; }
}
