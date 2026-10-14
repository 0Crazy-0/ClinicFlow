using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Common.Models;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Appointments.Queries.GetAdultFamilyMemberAppointmentsByPatientId;

public sealed class GetAdultFamilyMemberAppointmentsByPatientIdQueryHandler(
    IFamilyMembershipRepository familyMembershipRepository,
    IPatientRepository patientRepository,
    IAppointmentRepository appointmentRepository,
    TimeProvider timeProvider
)
    : IRequestHandler<
        GetAdultFamilyMemberAppointmentsByPatientIdQuery,
        PaginatedList<PatientAppointmentDto>
    >
{
    public async Task<PaginatedList<PatientAppointmentDto>> Handle(
        GetAdultFamilyMemberAppointmentsByPatientIdQuery request,
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

        if (patientAge < DomainRules.AdultAge)
            throw new DomainValidationException(DomainErrors.Appointment.AdultHistoryRequiresAdult);

        var (items, totalCount) = await appointmentRepository.GetByPatientIdInCategoriesAsync(
            request.PatientId,
            allowedCategories,
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
                a.TimeSlot.Start,
                a.TimeSlot.End,
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
