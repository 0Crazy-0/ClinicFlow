using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Appointments.Queries.GetAppointmentsByPatientId;

public sealed class GetAppointmentsByPatientIdQueryHandler(
    IAppointmentRepository appointmentRepository
) : IRequestHandler<GetAppointmentsByPatientIdQuery, IEnumerable<AppointmentDto>>
{
    public async Task<IEnumerable<AppointmentDto>> Handle(
        GetAppointmentsByPatientIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var appointments = await appointmentRepository.GetByPatientIdAsync(
            request.PatientId,
            cancellationToken
        );

        return appointments.Select(a => new AppointmentDto(
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
        ));
    }
}
