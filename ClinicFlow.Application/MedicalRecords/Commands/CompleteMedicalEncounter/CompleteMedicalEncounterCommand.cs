using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Commands.CompleteMedicalEncounter;

public sealed record CompleteMedicalEncounterCommand(
    Guid DoctorId,
    Guid AppointmentId,
    Guid MedicalRecordId
) : IRequest<Guid>;
