namespace ControlVencimientosP.Domain;

/// <summary>
/// El tenant. Hoy hay una sola fila (Id = 1), pero la tabla existe desde el
/// dia uno para que migrar a SaaS sea configuracion y no una reescritura.
/// No implementa IEntidadDeEmpresa: ella ES la empresa.
/// </summary>
public class Empresa
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Cuit { get; set; }

    /// <summary>Id IANA. Todo calculo de "vence hoy" se hace contra esta zona.</summary>
    public string ZonaHoraria { get; set; } = "America/Argentina/Buenos_Aires";

    public string? LogoUrl { get; set; }
    public bool Activa { get; set; } = true;
    public DateTime CreadaEn { get; set; }

    public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
    public ICollection<Item> Items { get; set; } = new List<Item>();
}
