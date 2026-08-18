namespace ControlVencimientosP.Domain;

/// <summary>
/// La cosa que se controla: el matafuego Nro 182, la habilitacion municipal,
/// el carnet de Juan Perez. Persiste a lo largo de las renovaciones; lo que
/// cambia es el <see cref="Vencimiento"/> asociado.
/// </summary>
public class Item : IEntidadDeEmpresa
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int CategoriaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    /// <summary>Numero visible para el usuario. Ej: "182" en "matafuego Nro 182".</summary>
    public string? Codigo { get; set; }

    public string? Ubicacion { get; set; }
    public string? ResponsableUsuarioId { get; set; }
    public string? Proveedor { get; set; }
    public string? Notas { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime CreadoEn { get; set; }

    public Empresa? Empresa { get; set; }
    public Categoria? Categoria { get; set; }
    public Usuario? ResponsableUsuario { get; set; }
    public ICollection<Vencimiento> Vencimientos { get; set; } = new List<Vencimiento>();
}
