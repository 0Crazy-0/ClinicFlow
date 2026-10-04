using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Common.Models;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Appointments.Queries.GetOwnAppointmentsByPatientId;

public sealed class GetOwnAppointmentsByPatientIdQueryHandler(
    IFamilyMembershipRepository familyMembershipRepository,
    IAppointmentRepository appointmentRepository
) : IRequestHandler<GetOwnAppointmentsByPatientIdQuery, PaginatedList<AppointmentDto>>
{
    /// <inheritdoc />
    public async Task<PaginatedList<AppointmentDto>> Handle(
        GetOwnAppointmentsByPatientIdQuery request,
        CancellationToken cancellationToken
    )
    {
        if (
            !await familyMembershipRepository.HasActiveSelfMembershipAsync(
                request.RequesterUserId,
                request.PatientId,
                cancellationToken
            )
        )
            throw new DomainValidationException(DomainErrors.Appointment.UnauthorizedAccess);

        var (items, totalCount) = await appointmentRepository.GetByPatientIdPaginatedAsync(
            request.PatientId,
            request.PageNumber,
            request.PageSize,
            cancellationToken
        );

        var dtos = items
            .Select(a => new AppointmentDto(
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

        return new PaginatedList<AppointmentDto>(
            dtos,
            totalCount,
            request.PageNumber,
            request.PageSize
        );
    }
}
