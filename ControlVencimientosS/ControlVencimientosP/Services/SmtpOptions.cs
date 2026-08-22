namespace ControlVencimientosP.Services;

/// <summary>
/// Config del servidor SMTP que manda los avisos. Los valores reales viven en
/// User Secrets en desarrollo (<c>dotnet user-secrets set "Smtp:Usuario" "..."</c>)
/// o en la configuracion del hosting en produccion (Azure App Settings, Key
/// Vault, etc.): nunca en appsettings.json, que se versiona en git.
/// </summary>
public class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Puerto { get; set; } = 587;
    public string Usuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    /// <summary>Direccion que ve el destinatario en "De". Suele coincidir con Usuario.</summary>
    public string RemitenteEmail { get; set; } = string.Empty;

    public string RemitenteNombre { get; set; } = "Control de Vencimientos";
    public bool UsarSsl { get; set; } = true;
}
