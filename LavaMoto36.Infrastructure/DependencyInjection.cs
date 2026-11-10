using LavaMoto365.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LavaMoto365.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLavaMotoInfrastructure(this IServiceCollection services, string connectionString)
    {
        // En Blazor Server se usa IDbContextFactory: un DbContext por operación,
        // no uno por circuito (los circuitos viven mucho tiempo).
        services.AddDbContextFactory<LavaMotoDbContext>(o =>
            o.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory_LavaMoto"))); // no choca con las migraciones de Identity

        // --- Tania ---
        services.AddScoped<ProveedorService>();
        services.AddScoped<OrdenCompraService>();

        // --- Daniel: services.AddScoped<InventarioService>(); ---
        return services;
    }
}
