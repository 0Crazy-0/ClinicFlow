using ClinicFlow.Application.Patients.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Patients.Queries.GetPatientsByUserId;

public sealed class GetPatientsByUserIdQueryHandler(IPatientRepository patientRepository)
    : IRequestHandler<GetPatientsByUserIdQuery, IReadOnlyList<PatientDto>>
{
    public async Task<IReadOnlyList<PatientDto>> Handle(
        GetPatientsByUserIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var patients = await patientRepository.GetAllByUserIdAsync(
            request.UserId,
            cancellationToken
        );

        return
        [
            .. patients.Select(patient => new PatientDto(
                patient.Id,
                patient.UserId,
                patient.FullName.FullName,
                patient.RelationshipToUser,
                patient.DateOfBirth,
                patient.BloodType?.Value,
                patient.Allergies,
                patient.ChronicConditions,
                patient.EmergencyContact?.Name.ToString(),
                patient.EmergencyContact?.PhoneNumber.ToString()
            )),
        ];
    }
}
