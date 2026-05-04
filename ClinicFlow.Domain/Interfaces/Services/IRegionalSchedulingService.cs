using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Interfaces.Services;

/// <summary>
/// Defines the policy for enforcing regional and medical specialty regulations during appointment scheduling.
/// </summary>
public interface IRegionalSchedulingService
{
    SchedulingClearance EnforceSchedulingRegulations(
        Doctor targetDoctor,
        Patient targetPatient,
        AppointmentTypeDefinition appointmentType
    );
}
