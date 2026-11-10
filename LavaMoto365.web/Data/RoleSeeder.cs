
using Microsoft.AspNetCore.Identity;

namespace LavaMoto365.web.Data
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(
            IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            var userManager = serviceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles =
            {
                "Administrador",
                "Empleado",
                "Recepcionista"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(
                        new IdentityRole(role));

                    if (!result.Succeeded)
                    {
                        throw new Exception(
                            $"No se pudo crear el rol {role}.");
                    }
                }
            }

            // Asignar el rol Administrador a la cuenta inicial
            const string adminEmail = "prueba1@gmail.com";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                return; // aún no existe; se registra y se reinicia la app
            }

            if (!await userManager.IsInRoleAsync(
                adminUser, "Administrador"))
            {
                var result = await userManager.AddToRoleAsync(
                    adminUser, "Administrador");

                if (!result.Succeeded)
                {
                    throw new Exception(
                        "No se pudo asignar el rol Administrador.");
                }
            }
        }
    }
}
