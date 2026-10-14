using ClinicFlow.Application.Patients.Queries.DTOs;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Patients.Queries.GetPatientById;

public sealed class GetPatientByIdQueryHandler(IPatientRepository patientRepository)
    : IRequestHandler<GetPatientByIdQuery, PatientDto>
{
    /// <inheritdoc />
    public async Task<PatientDto> Handle(
        GetPatientByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var patient =
            await patientRepository.GetByIdAsync(request.PatientId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Patient),
                request.PatientId
            );

        return new PatientDto(
            patient.Id,
            patient.FullName.FullName,
            patient.DateOfBirth,
            patient.BloodGroup?.Value,
            patient.Allergies,
            patient.ChronicConditions,
            patient.Contact?.Name.ToString(),
            patient.Contact?.Phone.ToString(),
            patient.SexAtBirth
        );
    }
}
