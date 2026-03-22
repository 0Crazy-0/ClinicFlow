using ClinicFlow.Application.Patients.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Patients.Queries.GetPatientsByUserId;

public class GetPatientsByUserIdQueryHandler(IPatientRepository patientRepository)
    : IRequestHandler<GetPatientsByUserIdQuery, IEnumerable<PatientDto>>
{
    public async Task<IEnumerable<PatientDto>> Handle(
        GetPatientsByUserIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var patients = await patientRepository.GetAllByUserIdAsync(
            request.UserId,
            cancellationToken
        );

        return patients.Select(patient => new PatientDto(
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
        ));
    }
}
