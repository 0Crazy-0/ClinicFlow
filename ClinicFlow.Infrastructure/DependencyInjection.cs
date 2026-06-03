using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Infrastructure.Persistence;
using ClinicFlow.Infrastructure.Persistence.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var dbOptions = DatabaseOptions.FromConfiguration(configuration);

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(dbOptions.ConnectionString);

            if (dbOptions.SeedOnStartup)
            {
                options.UseSeeding(
                    (context, _) =>
                        Persistence.Seeding.DbSeeder.Seed(
                            (ApplicationDbContext)context,
                            TimeProvider.System
                        )
                );
                options.UseAsyncSeeding(
                    (context, _, cancellationToken) =>
                        Persistence.Seeding.DbSeeder.SeedAsync(
                            (ApplicationDbContext)context,
                            TimeProvider.System,
                            cancellationToken
                        )
                );
            }
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
