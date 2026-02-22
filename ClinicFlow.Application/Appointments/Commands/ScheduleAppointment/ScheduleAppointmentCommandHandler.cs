using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ScheduleAppointment;

public class ScheduleAppointmentCommandHandler(IPatientRepository patientRepository, IDoctorRepository doctorRepository, IPatientPenaltyRepository penaltyRepository,
    IScheduleRepository scheduleRepository, IAppointmentRepository appointmentRepository, AppointmentSchedulingService schedulingService, IUnitOfWork unitOfWork) : IRequestHandler<ScheduleAppointmentCommand, Guid>
{
    public async Task<Guid> Handle(ScheduleAppointmentCommand request, CancellationToken cancellationToken)
    {
        var patient = await patientRepository.GetByIdAsync(request.PatientId) ?? throw new EntityNotFoundException(nameof(Patient), request.PatientId);

        var doctor = await doctorRepository.GetByIdAsync(request.DoctorId) ?? throw new EntityNotFoundException(nameof(Doctor), request.DoctorId);

        var penalties = await penaltyRepository.GetByPatientIdAsync(request.PatientId);

        var timeRange = TimeRange.Create(request.StartTime, request.EndTime);

        var doctorSchedule = await scheduleRepository.GetByDoctorAndDayAsync(doctor.Id, request.ScheduledDate.DayOfWeek);

        var hasConflict = await appointmentRepository.HasConflictAsync(doctor.Id, request.ScheduledDate, timeRange);

        var appointment = schedulingService.ScheduleAppointment(patient, penalties, doctor, request.ScheduledDate, timeRange, request.AppointmentTypeId, doctorSchedule, hasConflict);

        await appointmentRepository.CreateAsync(appointment);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return appointment.Id;
    }
}
