namespace ControlVencimientosP.ViewModels;

/// <summary>Todo lo que necesita la pantalla de Configuración.</summary>
public class ConfiguracionViewModel
{
    // No es init: en el POST de EditarAvisos, si la validación falla, el
    // controller la reemplaza directamente por el modelo posteado (con lo
    // que escribió el usuario, errores incluidos) antes de volver a mostrar
    // la pantalla completa.
    public AvisosConfigViewModel Avisos { get; set; } = new();

    public IReadOnlyList<DestinatarioFila> Destinatarios { get; init; } = [];
    public IReadOnlyList<CategoriaFila> Categorias { get; init; } = [];
}
