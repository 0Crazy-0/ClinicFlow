using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Reassignment;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ReassignAppointment;

public sealed class ReassignAppointmentCommandHandler(
    IAppointmentRepository appointmentRepository,
    IDoctorRepository doctorRepository,
    IScheduleRepository scheduleRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ReassignAppointmentCommand>
{
    public async Task Handle(ReassignAppointmentCommand request, CancellationToken ct)
    {
        var appointment =
            await appointmentRepository.GetByIdAsync(request.AppointmentId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Appointment),
                request.AppointmentId
            );

        var newDoctor =
            await doctorRepository.GetByIdAsync(request.NewDoctorId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Doctor),
                request.NewDoctorId
            );

        var newTimeRange = TimeRange.Create(request.NewStartTime, request.NewEndTime);
        var newDoctorSchedule = await scheduleRepository.GetByDoctorAndDayAsync(
            newDoctor.Id,
            request.NewDate.DayOfWeek,
            ct
        );

        var hasConflict = await appointmentRepository.HasConflictAsync(
            newDoctor.Id,
            request.NewDate,
            newTimeRange,
            ct
        );

        AppointmentReassignmentService.Reassign(
            appointment,
            new AppointmentReassignmentArgs
            {
                NewDoctorId = newDoctor.Id,
                NewDate = request.NewDate,
                NewTimeRange = newTimeRange,
            },
            new AppointmentReassignmentContext
            {
                NewDoctorSchedule = newDoctorSchedule,
                HasConflict = hasConflict,
            }
        );

        await unitOfWork.SaveChangesAsync(ct);
    }
}
