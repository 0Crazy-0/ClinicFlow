using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Infrastructure.Persistence;
using ClinicFlow.Infrastructure.Persistence.Options;
using ClinicFlow.Infrastructure.Persistence.Repositories;
using ClinicFlow.Infrastructure.Time;
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
        var clinicOptions = ClinicOptions.FromConfiguration(configuration);

        var clinicTimeProvider = new ClinicTimeProvider(
            TimeProvider.System,
            clinicOptions.TimeZoneId
        );

        services.AddSingleton<TimeProvider>(clinicTimeProvider);

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(dbOptions.ConnectionString);

            if (dbOptions.SeedOnStartup)
            {
                options.UseSeeding(
                    (context, _) =>
                        Persistence.Seeding.DbSeeder.Seed(
                            (ApplicationDbContext)context,
                            clinicTimeProvider
                        )
                );
                options.UseAsyncSeeding(
                    (context, _, cancellationToken) =>
                        Persistence.Seeding.DbSeeder.SeedAsync(
                            (ApplicationDbContext)context,
                            clinicTimeProvider,
                            cancellationToken
                        )
                );
            }
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<
            IAppointmentTypeDefinitionRepository,
            AppointmentTypeDefinitionRepository
        >();
        services.AddScoped<IClinicalFormTemplateRepository, ClinicalFormTemplateRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
        services.AddScoped<IMedicalSpecialtyRepository, MedicalSpecialtyRepository>();
        services.AddScoped<IPatientPenaltyRepository, PatientPenaltyRepository>();
        services.AddScoped<IFamilyMembershipRepository, FamilyMembershipRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
