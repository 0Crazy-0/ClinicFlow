using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Common.Models;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services.Policies;
using MediatR;

namespace ClinicFlow.Application.Appointments.Queries.GetMinorFamilyMemberAppointmentsByPatientId;

/// <summary>
/// Queries the appointment history of a tutor's minor patients. Returns appointment
/// metadata only, without the patient's private notes. Tutor access only.
/// </summary>
public sealed class GetMinorFamilyMemberAppointmentsByPatientIdQueryHandler(
    IFamilyMembershipRepository familyMembershipRepository,
    IPatientRepository patientRepository,
    IAppointmentRepository appointmentRepository,
    IAppointmentConsentGrantRepository consentGrantRepository,
    TimeProvider timeProvider
)
    : IRequestHandler<
        GetMinorFamilyMemberAppointmentsByPatientIdQuery,
        PaginatedList<MinorFamilyAppointmentDto>
    >
{
    public async Task<PaginatedList<MinorFamilyAppointmentDto>> Handle(
        GetMinorFamilyMemberAppointmentsByPatientIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var membership =
            await familyMembershipRepository.GetActiveMembershipAsync(
                request.RequesterUserId,
                request.PatientId,
                cancellationToken
            )
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(FamilyMembership),
                request.PatientId
            );

        var patient =
            await patientRepository.GetByIdAsync(request.PatientId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Patient),
                request.PatientId
            );

        var referenceTime = timeProvider.GetLocalNow().DateTime;
        var patientAge = patient.GetAge(DateOnly.FromDateTime(referenceTime));

        if (patientAge >= DomainRules.AdultAge)
            throw new DomainValidationException(DomainErrors.Appointment.MinorHistoryRequiresMinor);

        membership.EnsureGuardianAccess();

        var excludedCategories = ProtectedCategoryPolicy.GetProtectedCategoriesFor(patientAge);

        var authorizedCategories = await consentGrantRepository.GetEffectiveCategoriesAsync(
            request.PatientId,
            membership.Id,
            referenceTime,
            cancellationToken
        );

        var authorizedAppointmentIds = await consentGrantRepository.GetEffectiveAppointmentIdsAsync(
            request.PatientId,
            membership.Id,
            referenceTime,
            cancellationToken
        );

        var (items, totalCount) = await appointmentRepository.GetMinorVisibleAppointmentsAsync(
            request.PatientId,
            excludedCategories,
            authorizedCategories,
            authorizedAppointmentIds,
            request.RequesterUserId,
            request.PageNumber,
            request.PageSize,
            cancellationToken
        );

        var dtos = items
            .Select(a => new MinorFamilyAppointmentDto(
                a.Id,
                a.PatientId,
                a.DoctorId,
                a.AppointmentTypeId,
                a.ScheduledDate,
                a.TimeSlot.Start,
                a.TimeSlot.End,
                a.Status,
                a.ReceptionistNotes,
                a.GuardianNotes
            ))
            .ToList();

        return new PaginatedList<MinorFamilyAppointmentDto>(
            dtos,
            totalCount,
            request.PageNumber,
            request.PageSize
        );
    }
}
