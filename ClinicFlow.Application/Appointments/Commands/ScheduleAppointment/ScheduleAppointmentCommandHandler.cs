using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ScheduleAppointment;

public class ScheduleAppointmentCommandHandler(IPatientPenaltyRepository penaltyRepository,
    IPatientRepository patientRepository, IScheduleRepository scheduleRepository, IAppointmentRepository appointmentRepository, 
    IUnitOfWork unitOfWork) : IRequestHandler<ScheduleAppointmentCommand, Guid>
{
    public async Task<Guid> Handle(ScheduleAppointmentCommand request, CancellationToken cancellationToken)
    {
        var patient = await patientRepository.GetByIdAsync(request.PatientId, cancellationToken)
            ?? throw new EntityNotFoundException(DomainErrors.General.NotFound, nameof(Patient), request.PatientId);

        var penalties = await penaltyRepository.GetByPatientIdAsync(request.PatientId, cancellationToken);

        var timeRange = TimeRange.Create(request.StartTime, request.EndTime);

        var doctorSchedule = await scheduleRepository.GetByDoctorAndDayAsync(request.DoctorId, request.ScheduledDate.DayOfWeek, cancellationToken);

        var hasConflict = await appointmentRepository.HasConflictAsync(request.DoctorId, request.ScheduledDate, timeRange, cancellationToken);

        var context = new AppointmentSchedulingContext
        {
            Penalties = penalties,
            DoctorSchedule = doctorSchedule,
            HasConflict = hasConflict
        };

        var appointmentDetails = new AppointmentSchedulingDetails
        {
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            ScheduledDate = request.ScheduledDate,
            TimeRange = timeRange,
            AppointmentTypeId = request.AppointmentTypeId
        };

        var appointment = AppointmentSchedulingService.ScheduleAppointment(patient, appointmentDetails, context);

        await appointmentRepository.CreateAsync(appointment, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return appointment.Id;
    }
}
