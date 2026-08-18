using ControlVencimientosP.Data;
using ControlVencimientosP.Domain;
using ControlVencimientosP.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

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
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opciones =>
{
    opciones.LoginPath = "/Cuenta/Login";
    opciones.AccessDeniedPath = "/Cuenta/SinAcceso";
    opciones.ExpireTimeSpan = TimeSpan.FromDays(14);
    opciones.SlidingExpiration = true;
});

var app = builder.Build();

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

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
