using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Infrastructure.Persistence.Seeding;

public record AppointmentSeedingArgs(
    IReadOnlyList<Patient> Patients,
    IReadOnlyList<Doctor> Doctors,
    IReadOnlyList<AppointmentTypeDefinition> AppointmentTypes,
    IReadOnlyList<Schedule> Schedules,
    IReadOnlyList<User> PatientUsers
);
