using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ScheduleAppointment;

public class ScheduleAppointmentCommandHandler(IPatientPenaltyRepository penaltyRepository,
    IScheduleRepository scheduleRepository, IAppointmentRepository appointmentRepository, IUnitOfWork unitOfWork) : IRequestHandler<ScheduleAppointmentCommand, Guid>
{
    public async Task<Guid> Handle(ScheduleAppointmentCommand request, CancellationToken cancellationToken)
    {

        var penalties = await penaltyRepository.GetByPatientIdAsync(request.PatientId);

        var timeRange = TimeRange.Create(request.StartTime, request.EndTime);

        var doctorSchedule = await scheduleRepository.GetByDoctorAndDayAsync(request.DoctorId, request.ScheduledDate.DayOfWeek);

        var hasConflict = await appointmentRepository.HasConflictAsync(request.DoctorId, request.ScheduledDate, timeRange);

        var context = new AppointmentSchedulingContext
        {
            Penalties = penalties,
            DoctorSchedule = doctorSchedule,
            HasConflict = hasConflict
        };

        var appointment = AppointmentSchedulingService.ScheduleAppointment(request.PatientId, request.DoctorId, request.ScheduledDate, timeRange,
            request.AppointmentTypeId, context);

        await appointmentRepository.CreateAsync(appointment);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return appointment.Id;
    }
}
