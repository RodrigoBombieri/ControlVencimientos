using System.Linq.Expressions;
using ControlVencimientosP.Domain;
using ControlVencimientosP.Tenancy;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ControlVencimientosP.Data;

public class AppDbContext : IdentityDbContext<Usuario>
{
    private readonly ITenantProvider _tenant;

    public AppDbContext(DbContextOptions<AppDbContext> opciones, ITenantProvider tenant)
        : base(opciones) => _tenant = tenant;

    /// <summary>
    /// La lee el query filter en cada consulta. Tiene que ser una propiedad del
    /// contexto y no una variable capturada: EF compila el filtro una sola vez,
    /// pero re-evalua las propiedades del contexto en cada query. Si aca hubiera
    /// una constante, la segunda empresa veria los datos de la primera.
    /// </summary>
    public int EmpresaIdActual => _tenant.EmpresaId;

    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Vencimiento> Vencimientos => Set<Vencimiento>();
    public DbSet<Adjunto> Adjuntos => Set<Adjunto>();
    public DbSet<Destinatario> Destinatarios => Set<Destinatario>();
    public DbSet<AvisoEnviado> AvisosEnviados => Set<AvisoEnviado>();
    public DbSet<ConfiguracionAvisos> ConfiguracionesAvisos => Set<ConfiguracionAvisos>();

    protected override void OnModelCreating(ModelBuilder constructor)
    {
        base.OnModelCreating(constructor);

        constructor.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        AplicarFiltrosDeEmpresa(constructor);
        SeedInicial.Aplicar(constructor);
    }

    /// <summary>
    /// Le pone el filtro por empresa a toda entidad que implemente
    /// <see cref="IEntidadDeEmpresa"/>, por reflexion. Hacerlo asi y no a mano
    /// entidad por entidad es lo que garantiza que nadie se olvide de una:
    /// una entidad nueva queda filtrada por el solo hecho de implementar la
    /// interfaz.
    /// </summary>
    private void AplicarFiltrosDeEmpresa(ModelBuilder constructor)
    {
        foreach (var tipo in constructor.Model.GetEntityTypes())
        {
            if (!typeof(IEntidadDeEmpresa).IsAssignableFrom(tipo.ClrType))
                continue;

            // e => e.EmpresaId == this.EmpresaIdActual
            var parametro = Expression.Parameter(tipo.ClrType, "e");
            var cuerpo = Expression.Equal(
                Expression.Property(parametro, nameof(IEntidadDeEmpresa.EmpresaId)),
                Expression.Property(Expression.Constant(this), nameof(EmpresaIdActual)));

            constructor.Entity(tipo.ClrType)
                       .HasQueryFilter(Expression.Lambda(cuerpo, parametro));
        }
    }

    public override int SaveChanges()
    {
        EstamparEmpresaId();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancelacion = default)
    {
        EstamparEmpresaId();
        return base.SaveChangesAsync(cancelacion);
    }

    /// <summary>
    /// Completa el EmpresaId de lo que se esta insertando. Sin esto, una entidad
    /// nueva se guardaria con EmpresaId = 0 y despues el query filter la haria
    /// desaparecer: el clasico "lo guarde y no aparece en la lista".
    /// </summary>
    private void EstamparEmpresaId()
    {
        foreach (var entrada in ChangeTracker.Entries<IEntidadDeEmpresa>())
        {
            if (entrada.State == EntityState.Added && entrada.Entity.EmpresaId == 0)
                entrada.Entity.EmpresaId = EmpresaIdActual;
        }
    }
}
