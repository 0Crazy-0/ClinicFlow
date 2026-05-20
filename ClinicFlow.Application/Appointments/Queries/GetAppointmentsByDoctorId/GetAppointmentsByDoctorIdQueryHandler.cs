using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Common.Models;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDoctorId;

public sealed class GetAppointmentsByDoctorIdQueryHandler(
    IAppointmentRepository appointmentRepository
) : IRequestHandler<GetAppointmentsByDoctorIdQuery, PaginatedList<AppointmentDto>>
{
    /// <inheritdoc />
    public async Task<PaginatedList<AppointmentDto>> Handle(
        GetAppointmentsByDoctorIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var (items, totalCount) = await appointmentRepository.GetByDoctorIdPaginatedAsync(
            request.DoctorId,
            request.Date,
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
