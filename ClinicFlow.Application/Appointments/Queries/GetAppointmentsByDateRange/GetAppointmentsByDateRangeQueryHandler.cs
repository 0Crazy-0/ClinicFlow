using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Common.Models;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDateRange;

public sealed class GetAppointmentsByDateRangeQueryHandler(
    IAppointmentRepository appointmentRepository
) : IRequestHandler<GetAppointmentsByDateRangeQuery, PaginatedList<AppointmentDto>>
{
    public async Task<PaginatedList<AppointmentDto>> Handle(
        GetAppointmentsByDateRangeQuery request,
        CancellationToken cancellationToken
    )
    {
        var (items, totalCount) = await appointmentRepository.GetByDateRangePaginatedAsync(
            request.StartDate,
            request.EndDate,
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
                a.ReceptionistNotes
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
