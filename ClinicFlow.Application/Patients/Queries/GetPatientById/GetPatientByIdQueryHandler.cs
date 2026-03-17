using ClinicFlow.Application.Patients.Queries.DTOs;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Patients.Queries.GetPatientById;

public class GetPatientByIdQueryHandler(IPatientRepository patientRepository) : IRequestHandler<GetPatientByIdQuery, PatientDto>
{
    public async Task<PatientDto> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        var patient = await patientRepository.GetByIdAsync(request.PatientId, cancellationToken)
            ?? throw new EntityNotFoundException(DomainErrors.General.NotFound, nameof(Patient), request.PatientId);

        return new PatientDto(patient.Id, patient.UserId, patient.FullName.FullName, patient.RelationshipToUser, patient.DateOfBirth,
             patient.BloodType.Value, patient.Allergies, patient.ChronicConditions);
    }
}
