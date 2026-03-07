using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDateRange;

public class GetAppointmentsByDateRangeQueryHandler(IAppointmentRepository appointmentRepository) : IRequestHandler<GetAppointmentsByDateRangeQuery, IEnumerable<AppointmentDto>>
{
    public async Task<IEnumerable<AppointmentDto>> Handle(GetAppointmentsByDateRangeQuery request, CancellationToken cancellationToken)
    {
        var appointments = await appointmentRepository.GetByDateRangeAsync(request.StartDate, request.EndDate, cancellationToken);

        return appointments.Select(a => new AppointmentDto(a.Id, a.PatientId, a.DoctorId, a.AppointmentTypeId, a.ScheduledDate,
            a.TimeRange.Start, a.TimeRange.End, a.Status, a.PatientNotes, a.ReceptionistNotes));
    }
}
