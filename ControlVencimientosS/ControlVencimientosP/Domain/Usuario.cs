using Microsoft.AspNetCore.Identity;

namespace ControlVencimientosP.Domain;

/// <summary>
/// Usuario de la aplicacion.
/// </summary>
/// <remarks>
/// OJO: a proposito NO implementa <see cref="IEntidadDeEmpresa"/>, aunque tenga
/// EmpresaId. Un query filter global sobre el usuario rompe el login: Identity
/// necesita buscar al usuario por email ANTES de saber a que empresa pertenece,
/// y el filtro lo haria invisible. Cuando haya que filtrar usuarios por empresa,
/// se hace explicito en la consulta.
/// </remarks>
public class Usuario : IdentityUser
{
    public int EmpresaId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public bool RecibeAvisos { get; set; } = true;
    public bool Activo { get; set; } = true;

    public Empresa? Empresa { get; set; }
}
