using System.Net;

namespace ControlVencimientosP.Services;

/// <summary>Un item ya resuelto, listo para nombrarlo en un mail.</summary>
public record ItemParaMail(int VencimientoId, string Nombre, string? Codigo, int DiasRestantes);

/// <summary>Asunto y cuerpo HTML listos para mandar.</summary>
public record MailArmado(string Asunto, string CuerpoHtml);

/// <summary>
/// Arma el texto de los mails de aviso. Funcion pura, HTML escrito a mano
/// (sin motor de plantillas): son dos formatos fijos, no vale la pena la
/// dependencia. Todo texto que viene del usuario (nombre y codigo del item)
/// se HTML-encodea antes de entrar al mail: sin eso, un item cargado con
/// "&lt;script&gt;..." en el nombre rompe el mail de quien lo reciba.
/// </summary>
public static class ArmadorDeMailDeAviso
{
    public static MailArmado ArmarDigest(IReadOnlyList<ItemParaMail> items, DateOnly hoy)
    {
        var asunto = $"Vencimientos: {items.Count} pendiente{(items.Count == 1 ? "" : "s")}";

        var cuerpo = $"""
            <p>Resumen de vencimientos al {hoy:dd/MM/yyyy}:</p>
            <table cellpadding="6" cellspacing="0" style="border-collapse:collapse;width:100%">
              <thead>
                <tr style="text-align:left;border-bottom:2px solid #ddd">
                  <th>Item</th>
                  <th>Estado</th>
                </tr>
              </thead>
              <tbody>
                {FilasHtml(items)}
              </tbody>
            </table>
            """;

        return new MailArmado(asunto, cuerpo);
    }

    public static MailArmado ArmarAlertaIndividual(IReadOnlyList<ItemParaMail> items, DateOnly hoy)
    {
        var asunto = items.Count == 1
            ? $"Vencimiento urgente: {items[0].Nombre}"
            : $"{items.Count} vencimientos urgentes";

        var cuerpo = $"""
            <p><strong>Atencion:</strong> los siguientes vencimientos son urgentes (al {hoy:dd/MM/yyyy}):</p>
            <table cellpadding="6" cellspacing="0" style="border-collapse:collapse;width:100%">
              <tbody>
                {FilasHtml(items)}
              </tbody>
            </table>
            """;

        return new MailArmado(asunto, cuerpo);
    }

    private static string FilasHtml(IReadOnlyList<ItemParaMail> items)
        => string.Join("", items.OrderBy(i => i.DiasRestantes).Select(FilaHtml));

    private static string FilaHtml(ItemParaMail item)
    {
        var nombre = WebUtility.HtmlEncode(item.Nombre);
        var codigo = string.IsNullOrWhiteSpace(item.Codigo) ? "" : $" Nº {WebUtility.HtmlEncode(item.Codigo)}";
        var estado = WebUtility.HtmlEncode(PresentacionSemaforo.Relativo(item.DiasRestantes));

        return $"""<tr style="border-bottom:1px solid #eee"><td>{nombre}{codigo}</td><td>{estado}</td></tr>""";
    }
}
