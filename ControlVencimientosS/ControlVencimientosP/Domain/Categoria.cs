namespace ControlVencimientosP.Domain;

/// <summary>
/// El "Etc." del pedido original. En vez de una tabla por tipo de vencimiento,
/// hay una categoria configurable: el cliente agrega las suyas sin que nadie
/// programe nada. Es la decision de diseno central del producto.
/// </summary>
public class Categoria : IEntidadDeEmpresa
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    /// <summary>Un emoji, elegido de <see cref="Services.ArmadorDeConfiguracion.IconosDisponibles"/>.</summary>
    public string Icono { get; set; } = "📄";

    /// <summary>Color hex para la UI. No se usa para el semaforo.</summary>
    public string Color { get; set; } = "#2a78d6";

    /// <summary>Dias de anticipacion por defecto para los items de esta categoria.</summary>
    public int DiasAvisoDefault { get; set; } = 30;

    public int Orden { get; set; }
    public bool Activa { get; set; } = true;

    public Empresa? Empresa { get; set; }
    public ICollection<Item> Items { get; set; } = new List<Item>();
}
