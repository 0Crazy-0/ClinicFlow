using System.Linq.Expressions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the ClinicFlow application.
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientPenalty> PatientPenalties => Set<PatientPenalty>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentTypeDefinition> AppointmentTypes => Set<AppointmentTypeDefinition>();
    public DbSet<ClinicalFormTemplate> ClinicalFormTemplates => Set<ClinicalFormTemplate>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<MedicalSpecialty> MedicalSpecialties => Set<MedicalSpecialty>();
    public DbSet<Schedule> Schedules => Set<Schedule>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).Ignore(nameof(BaseEntity.DomainEvents));

                // HasQueryFilter requires a typed lambda (e => e.IsDeleted == false), but the entity type
                // is only known at runtime, so the expression tree must be built dynamically via Expression API.
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var body = Expression.Equal(
                    Expression.Property(parameter, nameof(BaseEntity.IsDeleted)),
                    Expression.Constant(false)
                );
                var lambda = Expression.Lambda(body, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
