namespace ControlVencimientosP.Domain;

/// <summary>El PDF del certificado. El archivo vive afuera; aca solo la referencia.</summary>
public class Adjunto : IEntidadDeEmpresa
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int VencimientoId { get; set; }

    public string NombreArchivo { get; set; } = string.Empty;

    /// <summary>
    /// Clave en el storage. Hoy una ruta local; en Azure, el blob name.
    /// Nunca guardar el archivo en la base: infla los backups y encarece todo.
    /// </summary>
    public string RutaBlob { get; set; } = string.Empty;

    public string? ContentType { get; set; }
    public long TamanioBytes { get; set; }
    public DateTime SubidoEn { get; set; }

    public Vencimiento? Vencimiento { get; set; }
}
