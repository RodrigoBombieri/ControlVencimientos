namespace ControlVencimientosP.Domain;

/// <summary>
/// Marca una entidad que pertenece a una empresa (tenant).
/// El <c>AppDbContext</c> le aplica automaticamente un query filter por EmpresaId
/// y le completa el valor al guardar. Toda entidad nueva del dominio deberia
/// implementarla: es lo que hace que pasar a SaaS no requiera tocar consultas.
/// </summary>
public interface IEntidadDeEmpresa
{
    int EmpresaId { get; set; }
}
