using ControlVencimientosP.Domain;
using Microsoft.EntityFrameworkCore;

namespace ControlVencimientosP.Data;

/// <summary>
/// Datos que van en la migracion para que la base arranque usable: la empresa,
/// su configuracion de avisos y las categorias del pedido original.
/// Todo con valores fijos, sin DateTime.Now: las migraciones tienen que ser
/// deterministicas o EF genera una migracion nueva en cada build.
/// </summary>
public static class SeedInicial
{
    public const int EmpresaId = 1;

    private static readonly DateTime Epoca = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void Aplicar(ModelBuilder constructor)
    {
        constructor.Entity<Empresa>().HasData(new Empresa
        {
            Id = EmpresaId,
            Nombre = "Mi empresa",
            ZonaHoraria = "America/Argentina/Buenos_Aires",
            Activa = true,
            CreadaEn = Epoca
        });

        constructor.Entity<ConfiguracionAvisos>().HasData(new ConfiguracionAvisos
        {
            EmpresaId = EmpresaId,
            HitosDias = "60,30,15,7,1,0",
            HoraEnvio = new TimeOnly(8, 0),
            ReenviarVencidosCadaDias = 7,
            DigestActivo = true,
            AlertaIndividualActiva = true,
            WhatsappActivo = false
        });

        constructor.Entity<Categoria>().HasData(Categorias());
    }

    /// <summary>
    /// Las categorias iniciales. El usuario puede editarlas, desactivarlas o
    /// agregar las suyas: por eso "Otro" al final y por eso no hay un enum.
    /// </summary>
    private static Categoria[] Categorias() =>
    [
        Cat( 1, "Habilitacion",   "building-2",       30),
        Cat( 2, "Seguro",         "shield",           60),
        Cat( 3, "Matafuego",      "flame-kindling",   30),
        Cat( 4, "ART",            "shield-plus",      30),
        Cat( 5, "Capacitacion",   "graduation-cap",   30),
        Cat( 6, "Carnet",         "id-card",          30),
        Cat( 7, "Licencia",       "scroll-text",      30),
        Cat( 8, "Mantenimiento",  "wrench",           15),
        Cat( 9, "Inspeccion",     "search-check",     30),
        Cat(10, "Contrato",       "file-signature",   60),
        Cat(11, "Otro",           "file-text",        30)
    ];

    private static Categoria Cat(int id, string nombre, string icono, int diasAviso) => new()
    {
        Id = id,
        EmpresaId = EmpresaId,
        Nombre = nombre,
        Icono = icono,
        Color = "#2a78d6",
        DiasAvisoDefault = diasAviso,
        Orden = id,
        Activa = true
    };
}
