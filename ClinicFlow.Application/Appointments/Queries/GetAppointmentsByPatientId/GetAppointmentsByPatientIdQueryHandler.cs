using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Appointments.Queries.GetAppointmentsByPatientId;

public sealed class GetAppointmentsByPatientIdQueryHandler(
    IAppointmentRepository appointmentRepository
) : IRequestHandler<GetAppointmentsByPatientIdQuery, IReadOnlyList<AppointmentDto>>
{
    public async Task<IReadOnlyList<AppointmentDto>> Handle(
        GetAppointmentsByPatientIdQuery request,
        CancellationToken ct
    )
    {
        var appointments = await appointmentRepository.GetByPatientIdAsync(request.PatientId, ct);

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
