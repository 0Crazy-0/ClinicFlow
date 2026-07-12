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

        foreach (
            var clrType in modelBuilder
                .Model.GetEntityTypes()
                .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType))
                .Select(e => e.ClrType)
        )
        {
            modelBuilder.Entity(clrType).Ignore(nameof(BaseEntity.DomainEvents));
            modelBuilder
                .Entity(clrType)
                .Property(nameof(BaseEntity.SequenceNumber))
                .ValueGeneratedOnAdd();

            modelBuilder.Entity(clrType).Property(nameof(BaseEntity.Version)).IsRowVersion();
        }

        foreach (
            var clrType in modelBuilder
                .Model.GetEntityTypes()
                .Where(e => typeof(SoftDeletableEntity).IsAssignableFrom(e.ClrType))
                .Select(e => e.ClrType)
        )
        {
            // HasQueryFilter requires a typed lambda (e => e.IsDeleted == false), but the entity type
            // is only known at runtime, so the expression tree must be built dynamically via Expression API.
            var parameter = Expression.Parameter(clrType, "e");
            var body = Expression.Equal(
                Expression.Property(parameter, nameof(SoftDeletableEntity.IsDeleted)),
                Expression.Constant(false)
            );
            var lambda = Expression.Lambda(body, parameter);
            modelBuilder.Entity(clrType).HasQueryFilter(lambda);
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
