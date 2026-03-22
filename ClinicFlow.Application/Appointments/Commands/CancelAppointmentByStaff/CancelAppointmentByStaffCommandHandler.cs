using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Cancellation;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointmentByStaff;

public sealed class CancelAppointmentByStaffCommandHandler(
    IAppointmentRepository appointmentRepository,
    IDoctorRepository doctorRepository,
    IMedicalSpecialtyRepository specialtyRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CancelAppointmentByStaffCommand>
{
    public async Task Handle(
        CancelAppointmentByStaffCommand request,
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

        AppointmentCancellationService.CancelByStaff(
            appointment,
            new StaffCancellationArgs(
                request.InitiatorUserId,
                request.InitiatorRole,
                specialty,
                request.Reason
            )
        );

        await appointmentRepository.UpdateAsync(appointment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
