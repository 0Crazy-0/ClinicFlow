using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Cancellation;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointmentByDoctor;

public sealed class CancelAppointmentByDoctorCommandHandler(
    TimeProvider timeProvider,
    IAppointmentRepository appointmentRepository,
    IDoctorRepository doctorRepository,
    IMedicalSpecialtyRepository specialtyRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CancelAppointmentByDoctorCommand>
{
    public async Task Handle(
        CancelAppointmentByDoctorCommand request,
        CancellationToken cancellationToken
    )
    {
        var appointment =
            await appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Appointment),
                request.AppointmentId
            );

        var doctor =
            await doctorRepository.GetByIdAsync(appointment.DoctorId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Doctor),
                appointment.DoctorId
            );

        var specialty =
            await specialtyRepository.GetByIdAsync(doctor.MedicalSpecialtyId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(MedicalSpecialty),
                doctor.MedicalSpecialtyId
            );

        var initiatorDoctor = await doctorRepository.GetByUserIdAsync(
            request.InitiatorUserId,
            cancellationToken
        );

        AppointmentCancellationService.CancelByDoctor(
            appointment,
            new DoctorCancellationArgs
            {
                InitiatorDoctor = initiatorDoctor,
                Specialty = specialty,
                Reason = request.Reason,
                CancelledAt = timeProvider.GetUtcNow().UtcDateTime,
            }
        );

        await appointmentRepository.UpdateAsync(appointment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
