using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace ControlVencimientosP.Services;

/// <summary>Un mail ya armado, listo para salir. Agnostico del proveedor a proposito.</summary>
public record MailParaEnviar(string Destinatario, string Asunto, string CuerpoHtml);

/// <summary>
/// Puerta de salida de mails. Una sola implementacion hoy (SMTP generico);
/// la interfaz existe separada para poder mockearla en tests si algun dia
/// hace falta, sin tocar quien la usa.
/// </summary>
public interface IEnviadorDeMail
{
    Task EnviarAsync(MailParaEnviar mail, CancellationToken cancelacion = default);
}

/// <summary>
/// Manda mails por SMTP generico (<see cref="SmtpClient"/> del BCL), sin
/// depender de ningun proveedor en particular: sirve con Gmail, con el SMTP
/// del hosting, o con cualquier proveedor que exponga SMTP (SendGrid,
/// Mailgun, etc. lo hacen todos).
/// </summary>
public class EnviadorDeMailSmtp : IEnviadorDeMail
{
    private readonly SmtpOptions _opciones;

    public EnviadorDeMailSmtp(IOptions<SmtpOptions> opciones) => _opciones = opciones.Value;

    public async Task EnviarAsync(MailParaEnviar mail, CancellationToken cancelacion = default)
    {
        using var mensaje = new MailMessage
        {
            From = new MailAddress(_opciones.RemitenteEmail, _opciones.RemitenteNombre),
            Subject = mail.Asunto,
            Body = mail.CuerpoHtml,
            IsBodyHtml = true
        };
        mensaje.To.Add(mail.Destinatario);

        using var cliente = new SmtpClient(_opciones.Host, _opciones.Puerto)
        {
            EnableSsl = _opciones.UsarSsl,
            Credentials = new NetworkCredential(_opciones.Usuario, _opciones.Password)
        };

        await cliente.SendMailAsync(mensaje, cancelacion);
    }
}
