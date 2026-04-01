using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDoctorId;

public sealed class GetAppointmentsByDoctorIdQueryHandler(
    IAppointmentRepository appointmentRepository
) : IRequestHandler<GetAppointmentsByDoctorIdQuery, IReadOnlyList<AppointmentDto>>
{
    public async Task<IReadOnlyList<AppointmentDto>> Handle(
        GetAppointmentsByDoctorIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var appointments = await appointmentRepository.GetByDoctorIdAsync(
            request.DoctorId,
            request.Date,
            cancellationToken
        );

        return
        [
            .. appointments.Select(a => new AppointmentDto(
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
            )),
        ];
    }
}
