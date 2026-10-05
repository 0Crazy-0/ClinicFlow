using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Common.Models;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services.Policies;
using MediatR;

namespace ClinicFlow.Application.Appointments.Queries.GetFamilyMemberAppointmentsByPatientId;

public sealed class GetFamilyMemberAppointmentsByPatientIdQueryHandler(
    IFamilyMembershipRepository familyMembershipRepository,
    IPatientRepository patientRepository,
    IAppointmentRepository appointmentRepository,
    TimeProvider timeProvider
)
    : IRequestHandler<
        GetFamilyMemberAppointmentsByPatientIdQuery,
        PaginatedList<PatientAppointmentDto>
    >
{
    /// <inheritdoc />
    public async Task<PaginatedList<PatientAppointmentDto>> Handle(
        GetFamilyMemberAppointmentsByPatientIdQuery request,
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

        var allowedCategories = membership.GetVisibleAppointmentCategories();

        var patient =
            await patientRepository.GetByIdAsync(request.PatientId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Patient),
                request.PatientId
            );

        var patientAge = patient.GetAge(DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime));

        var excludedCategories = ProtectedCategoryPolicy.GetProtectedCategoriesFor(patientAge);

        var (items, totalCount) =
            await appointmentRepository.GetByPatientIdInCategoriesExcludingProtectedAsync(
                request.PatientId,
                allowedCategories,
                excludedCategories,
                request.PageNumber,
                request.PageSize,
                cancellationToken
            );

        var dtos = items
            .Select(a => new PatientAppointmentDto(
                a.Id,
                a.PatientId,
                a.DoctorId,
                a.AppointmentTypeId,
                a.ScheduledDate,
                a.TimeRange.Start,
                a.TimeRange.End,
                a.Status,
                a.PatientNotes,
                a.ReceptionistNotes,
                a.GuardianNotes
            ))
            .ToList();

        return new PaginatedList<PatientAppointmentDto>(
            dtos,
            totalCount,
            request.PageNumber,
            request.PageSize
        );
    }
}
