using System.Security.Claims;
using ControlVencimientosP.Domain;
using ControlVencimientosP.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ControlVencimientosP.Data;

/// <summary>
/// Crea los roles y el primer administrador al arrancar, si no existen.
/// Es idempotente: se puede correr en cada arranque sin duplicar nada.
/// </summary>
public static class SeedIdentidad
{
    public static async Task AplicarAsync(IServiceProvider servicios)
    {
        using var alcance = servicios.CreateScope();
        var sp = alcance.ServiceProvider;

        var registro = sp.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(SeedIdentidad));
        var baseDatos = sp.GetRequiredService<AppDbContext>();

        // En un clon nuevo, la app puede arrancar antes del Update-Database.
        // Mejor avisar que reventar con una excepcion de conexion.
        if (!await baseDatos.Database.CanConnectAsync())
        {
            registro.LogWarning(
                "No se pudo conectar a la base, se omite el seed de identidad. Corre Update-Database.");
            return;
        }

        var roles = sp.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var rol in RolesApp.Todos)
        {
            if (!await roles.RoleExistsAsync(rol))
                await roles.CreateAsync(new IdentityRole(rol));
        }

        var usuarios = sp.GetRequiredService<UserManager<Usuario>>();

        // Si ya hay un administrador, no hay nada que hacer.
        if ((await usuarios.GetUsersInRoleAsync(RolesApp.Admin)).Count > 0)
            return;

        var configuracion = sp.GetRequiredService<IConfiguration>();
        var entorno = sp.GetRequiredService<IHostEnvironment>();

        var email = configuracion["Seed:AdminEmail"];
        var password = configuracion["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            // Un administrador con contrasena conocida y publicada en el codigo
            // es una puerta abierta. En produccion se prefiere no tener admin
            // (y que alguien lo note) antes que tener uno que cualquiera abre.
            if (!entorno.IsDevelopment())
            {
                registro.LogError(
                    "No hay administrador y faltan Seed:AdminEmail / Seed:AdminPassword. " +
                    "No se crea ninguno. Configuralos y volve a arrancar.");
                return;
            }

            email = string.IsNullOrWhiteSpace(email) ? "admin@local" : email;
            password = string.IsNullOrWhiteSpace(password) ? "Cambiar.123" : password;

            registro.LogWarning(
                "Creando el administrador de desarrollo {Email} con contrasena por defecto. " +
                "Configura Seed:AdminEmail y Seed:AdminPassword en User Secrets.", email);
        }

        var administrador = new Usuario
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            NombreCompleto = "Administrador",
            EmpresaId = SeedInicial.EmpresaId,
            RecibeAvisos = true,
            Activo = true
        };

        var resultado = await usuarios.CreateAsync(administrador, password);
        if (!resultado.Succeeded)
        {
            registro.LogError("No se pudo crear el administrador: {Errores}",
                string.Join(" | ", resultado.Errors.Select(e => e.Description)));
            return;
        }

        await usuarios.AddToRoleAsync(administrador, RolesApp.Admin);

        // El claim que hace andar el multiempresa. Hoy siempre vale 1, pero
        // dejarlo desde ahora significa que el dia que esto sea SaaS no hay que
        // tocar el login ni ninguna consulta: TenantProvider ya lo lee.
        await usuarios.AddClaimAsync(administrador,
            new Claim(TenantProvider.ClaimEmpresaId, SeedInicial.EmpresaId.ToString()));

        registro.LogInformation("Administrador creado: {Email}", email);
    }
}
