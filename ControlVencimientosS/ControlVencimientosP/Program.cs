using ControlVencimientosP.Data;
using ControlVencimientosP.Domain;
using ControlVencimientosP.Tenancy;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Logging (Serilog)
// ---------------------------------------------------------------------------
// Reemplaza al logger por defecto. ReadFrom.Configuration deja abierta la
// puerta a ajustar niveles desde appsettings.json (seccion "Serilog") sin
// tocar el codigo; mientras tanto, los niveles de siempre quedan puestos
// a mano para no perder el comportamiento actual.
builder.Host.UseSerilog((contexto, servicios, loggerConfig) => loggerConfig
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .ReadFrom.Configuration(contexto.Configuration)
    .ReadFrom.Services(servicios)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddControllersWithViews(opciones =>
{
    // Exige sesion en TODA la app por defecto. Un controller nuevo queda
    // protegido solo por existir; hay que marcarlo [AllowAnonymous] a
    // proposito para abrirlo (como se hace con CuentaController). Es al
    // reves de agregar [Authorize] uno por uno, que es facil de olvidar
    // justo en el controller nuevo que faltaba proteger.
    var exigirSesion = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    opciones.Filters.Add(new AuthorizeFilter(exigirSesion));
});

// Wiring nada mas: registra la infraestructura para que un futuro
// AbstractValidator<T> se descubra y se ejecute solo durante el model
// binding. Todavia no hay ningun validator escrito; los formularios
// actuales siguen validando con DataAnnotations.
builder.Services
    .AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ---------------------------------------------------------------------------
// Multiempresa
// ---------------------------------------------------------------------------
// Hoy hay una sola empresa y TenantProvider devuelve 1. El dia que esto sea
// SaaS, alcanza con emitir el claim "empresa_id" al loguear: los query filters
// del AppDbContext se encargan del resto y no hay que tocar ninguna consulta.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();

// ---------------------------------------------------------------------------
// Base de datos
// ---------------------------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(opciones =>
    opciones.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure()));

// ---------------------------------------------------------------------------
// Identidad
// ---------------------------------------------------------------------------
builder.Services
    .AddIdentity<Usuario, IdentityRole>(opciones =>
    {
        opciones.Password.RequiredLength = 8;
        opciones.Password.RequireNonAlphanumeric = false;
        opciones.User.RequireUniqueEmail = true;
        opciones.SignIn.RequireConfirmedAccount = false;
        opciones.Lockout.MaxFailedAccessAttempts = 10;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddClaimsPrincipalFactory<UsuarioClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(opciones =>
{
    opciones.LoginPath = "/Cuenta/Login";
    opciones.AccessDeniedPath = "/Cuenta/SinAcceso";
    opciones.ExpireTimeSpan = TimeSpan.FromDays(14);
    opciones.SlidingExpiration = true;
});

// ---------------------------------------------------------------------------
// Hangfire (trabajos en segundo plano)
// ---------------------------------------------------------------------------
// Por ahora solo la plomeria: storage en la misma base SQL Server y el
// dashboard protegido. El motor de avisos automaticos (el job que de verdad
// recorre los vencimientos y manda los emails) todavia no existe; se
// registra aca el dia que se construya, con RecurringJob.AddOrUpdate.
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

var app = builder.Build();

// Roles y primer administrador. Es idempotente: si ya existen, no hace nada.
await SeedIdentidad.AplicarAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// UseAuthentication va SIEMPRE antes de UseAuthorization. Al reves, el usuario
// llega sin identidad a la autorizacion y todo queda como anonimo.
app.UseAuthentication();
app.UseAuthorization();

// El dashboard de Hangfire tiene su propio pipeline de autorizacion, aparte
// del AuthorizeFilter global de MVC: sin este filtro, /hangfire quedaria
// visible para cualquiera que llegue a la URL.
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new SoloAdminHangfireFilter() }
});

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

/// <summary>Solo un Admin logueado puede ver /hangfire.</summary>
public class SoloAdminHangfireFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole(RolesApp.Admin);
    }
}
