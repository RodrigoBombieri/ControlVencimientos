namespace ControlVencimientosP.Domain;

/// <summary>
/// Quien recibe los avisos. No necesariamente es un usuario del sistema: suele
/// ser el gestor, el contador o el responsable de seguridad e higiene, que
/// nunca van a entrar a la app pero si necesitan el mail.
/// </summary>
public class Destinatario : IEntidadDeEmpresa
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }

    /// <summary>
    /// Ids de categoria separados por coma. Null o vacio = recibe todas.
    /// Ej: "1,3,8" para que solo le lleguen matafuegos, ART y capacitaciones.
    /// </summary>
    public string? CategoriasFiltro { get; set; }

    public bool Activo { get; set; } = true;

    public Empresa? Empresa { get; set; }

    /// <summary>Devuelve los ids de categoria del filtro. Lista vacia = todas.</summary>
    public IReadOnlyList<int> CategoriasFiltradas()
    {
        if (string.IsNullOrWhiteSpace(CategoriasFiltro))
            return [];

        return CategoriasFiltro
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => int.TryParse(t, out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();
    }

    /// <summary>True si a este destinatario le corresponde esa categoria.</summary>
    public bool CubreCategoria(int categoriaId)
    {
        var filtro = CategoriasFiltradas();
        return filtro.Count == 0 || filtro.Contains(categoriaId);
    }
}
