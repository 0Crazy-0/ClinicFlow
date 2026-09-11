using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Commands.SetGuardianInvolvementDeterminationByDoctor;

public sealed record SetGuardianInvolvementDeterminationByDoctorCommand(
    Guid MedicalRecordId,
    Guid InitiatorUserId,
    bool GuardianInvolvementDeemedAppropriate
) : IRequest;
