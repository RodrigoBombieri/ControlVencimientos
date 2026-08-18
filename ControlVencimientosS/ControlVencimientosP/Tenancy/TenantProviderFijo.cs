namespace ControlVencimientosP.Tenancy;

/// <summary>
/// Provider para contextos sin usuario logueado: jobs de Hangfire, seeds,
/// tests. El job diario recorre todas las empresas y crea un scope de DI con
/// uno de estos por cada una, para que los query filters sigan funcionando.
/// </summary>
public sealed class TenantProviderFijo : ITenantProvider
{
    public TenantProviderFijo(int empresaId) => EmpresaId = empresaId;

    public int EmpresaId { get; }
}
