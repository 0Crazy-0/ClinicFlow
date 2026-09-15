using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Commands.StartMedicalEncounter;

public sealed record StartMedicalEncounterCommand(
    Guid DoctorId,
    Guid AppointmentId,
    string ChiefComplaint,
    bool? GuardianInitiatedTreatment
) : IRequest<Guid>;
