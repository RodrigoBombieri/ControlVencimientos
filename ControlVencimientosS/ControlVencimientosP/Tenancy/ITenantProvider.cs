namespace ControlVencimientosP.Tenancy;

/// <summary>
/// Resuelve a que empresa pertenece la operacion en curso.
/// El <c>AppDbContext</c> lo consulta en cada query a traves del filtro global,
/// asi que ninguna consulta de la aplicacion tiene que acordarse de filtrar.
/// </summary>
public interface ITenantProvider
{
    int EmpresaId { get; }
}
