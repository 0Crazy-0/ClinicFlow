using ClinicFlow.Domain.Enums;
using MediatR;

namespace ClinicFlow.Application.Patients.Commands.CorrectBiologicalSexByDoctor;

public sealed record CorrectBiologicalSexByDoctorCommand(
    Guid PatientId,
    Guid InitiatorUserId,
    BiologicalSex CorrectedSex,
    string Reason
) : IRequest;
