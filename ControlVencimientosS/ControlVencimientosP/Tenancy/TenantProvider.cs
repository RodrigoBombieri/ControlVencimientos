using System.Security.Claims;

namespace ControlVencimientosP.Tenancy;

/// <summary>
/// Implementacion para el pipeline web.
/// </summary>
/// <remarks>
/// Hoy la app es de una sola empresa y esto devuelve siempre
/// <see cref="EmpresaUnica"/>. Cuando se pase a SaaS, alcanza con emitir el
/// claim <see cref="ClaimEmpresaId"/> al loguear: a partir de ahi cada usuario
/// ve solo lo suyo y no hay que tocar una sola consulta.
/// </remarks>
public class TenantProvider : ITenantProvider
{
    /// <summary>La unica empresa mientras la app sea monoempresa.</summary>
    public const int EmpresaUnica = 1;

    /// <summary>Claim que llevara el EmpresaId cuando esto sea SaaS.</summary>
    public const string ClaimEmpresaId = "empresa_id";

    private readonly IHttpContextAccessor _http;

    public TenantProvider(IHttpContextAccessor http) => _http = http;

    public int EmpresaId
    {
        get
        {
            var claim = _http.HttpContext?.User?.FindFirst(ClaimEmpresaId)?.Value;
            return int.TryParse(claim, out var id) && id > 0 ? id : EmpresaUnica;
        }
    }
}
