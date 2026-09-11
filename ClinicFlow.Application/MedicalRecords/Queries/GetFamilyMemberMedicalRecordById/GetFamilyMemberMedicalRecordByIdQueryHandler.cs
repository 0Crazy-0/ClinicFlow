using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services.Policies;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetFamilyMemberMedicalRecordById;

public sealed class GetFamilyMemberMedicalRecordByIdQueryHandler(
    IMedicalRecordRepository medicalRecordRepository,
    IFamilyMembershipRepository familyMembershipRepository,
    IPatientRepository patientRepository,
    TimeProvider timeProvider
) : IRequestHandler<GetFamilyMemberMedicalRecordByIdQuery, MedicalRecordDto>
{
    /// <inheritdoc />
    public async Task<MedicalRecordDto> Handle(
        GetFamilyMemberMedicalRecordByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var record =
            await medicalRecordRepository.GetByIdAsync(request.MedicalRecordId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(MedicalRecord),
                request.MedicalRecordId
            );

        var membership =
            await familyMembershipRepository.GetActiveMembershipAsync(
                request.RequesterUserId,
                record.PatientId,
                cancellationToken
            )
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(FamilyMembership),
                record.PatientId
            );

        membership.EnsureMedicalRecordsAccess();

        var patient =
            await patientRepository.GetByIdAsync(record.PatientId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Patient),
                record.PatientId
            );

        var patientAge = patient.GetAge(
            DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime)
        );

        if (
            ProtectedCategoryPolicy.IsProtectedForPatient(
                record.ProtectedCareCategory,
                patientAge,
                record.GuardianInitiatedTreatment,
                record.GuardianInvolvementDeemedAppropriate
            )
        )
            throw new DomainValidationException(DomainErrors.MedicalRecord.ProtectedByMinorConsent);

        return new MedicalRecordDto(
            record.Id,
            record.PatientId,
            record.DoctorId,
            record.AppointmentId,
            record.ChiefComplaint,
            [
                .. record.ClinicalDetails.Select(d => new ClinicalDetailDto(
                    d.TemplateCode,
                    d.JsonDataPayload
                )),
            ]
        );
    }
}
