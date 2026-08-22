using System.Security.Claims;
using ControlVencimientosP.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ControlVencimientosP.Data;

/// <summary>
/// Agrega el nombre completo como claim al armar el ClaimsPrincipal del
/// usuario logueado. Sin esto, _Layout.cshtml no tiene de donde sacar otra
/// cosa que el email (User.Identity.Name, que Identity completa solo con
/// eso por defecto).
/// </summary>
/// <remarks>
/// A diferencia de "empresa_id" (que se persiste una sola vez al crear el
/// usuario, con AddClaimAsync, porque no cambia nunca), NombreCompleto es
/// un dato editable de <see cref="Usuario"/>. Persistirlo tambien como
/// claim en AspNetUserClaims obligaria a mantener las dos copias
/// sincronizadas cada vez que alguien lo edite; calcularlo aca, en cambio,
/// siempre refleja el valor actual de la columna sin nada que sincronizar.
/// </remarks>
public class UsuarioClaimsPrincipalFactory : UserClaimsPrincipalFactory<Usuario, IdentityRole>
{
    public const string TipoClaimNombreCompleto = "nombre_completo";

    public UsuarioClaimsPrincipalFactory(
        UserManager<Usuario> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> opciones)
        : base(userManager, roleManager, opciones)
    {
    }

    public override async Task<ClaimsPrincipal> CreateAsync(Usuario usuario)
    {
        var principal = await base.CreateAsync(usuario);

        if (!string.IsNullOrWhiteSpace(usuario.NombreCompleto))
        {
            ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim(TipoClaimNombreCompleto, usuario.NombreCompleto));
        }

        return principal;
    }
}
