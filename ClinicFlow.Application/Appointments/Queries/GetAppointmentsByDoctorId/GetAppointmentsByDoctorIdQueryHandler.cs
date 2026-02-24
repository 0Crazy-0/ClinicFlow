using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDoctorId;

public class GetAppointmentsByDoctorIdQueryHandler(IAppointmentRepository appointmentRepository) : IRequestHandler<GetAppointmentsByDoctorIdQuery, IEnumerable<AppointmentDto>>
{
    public async Task<IEnumerable<AppointmentDto>> Handle(GetAppointmentsByDoctorIdQuery request, CancellationToken cancellationToken)
    {
        var appointments = await appointmentRepository.GetByDoctorIdAsync(request.DoctorId, request.Date);

        return appointments.Select(a => new AppointmentDto(a.Id, a.PatientId, a.DoctorId, a.AppointmentTypeId, a.ScheduledDate, 
            a.TimeRange.Start, a.TimeRange.End, a.Status, a.PatientNotes, a.ReceptionistNotes));
    }
}
