using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Appointments.Queries.GetAppointmentById;

public class GetAppointmentByIdQueryHandler(IAppointmentRepository appointmentRepository) : IRequestHandler<GetAppointmentByIdQuery, AppointmentDto>
{
    public async Task<AppointmentDto> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken)
            ?? throw new EntityNotFoundException(DomainErrors.General.NotFound, nameof(Appointment), request.AppointmentId);

        return new AppointmentDto(appointment.Id, appointment.PatientId, appointment.DoctorId, appointment.AppointmentTypeId, appointment.ScheduledDate,
            appointment.TimeRange.Start, appointment.TimeRange.End, appointment.Status, appointment.PatientNotes, appointment.ReceptionistNotes);
    }
}
