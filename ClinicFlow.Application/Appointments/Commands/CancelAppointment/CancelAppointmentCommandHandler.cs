using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Contexts;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointment;

public class CancelAppointmentCommandHandler(IAppointmentRepository appointmentRepository, IUserRepository userRepository,
    IAppointmentTypeDefinitionRepository appointmentTypeDefinitionRepository, IDoctorRepository doctorRepository, IPatientRepository patientRepository,
    IMedicalSpecialtyRepository medicalSpecialtyRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CancelAppointmentCommand>
{
    public async Task Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken)
            ?? throw new EntityNotFoundException(DomainErrors.General.NotFound, nameof(Appointment), request.AppointmentId);

        var initiator = await userRepository.GetByIdAsync(request.InitiatorUserId, cancellationToken) ??
            throw new EntityNotFoundException(DomainErrors.General.NotFound, nameof(User), request.InitiatorUserId);

        var appointmentType = await appointmentTypeDefinitionRepository.GetByIdAsync(appointment.AppointmentTypeId, cancellationToken)
            ?? throw new EntityNotFoundException(DomainErrors.General.NotFound, nameof(AppointmentTypeDefinition), appointment.AppointmentTypeId);

        var doctor = await doctorRepository.GetByIdAsync(appointment.DoctorId, cancellationToken) ??
             throw new EntityNotFoundException(DomainErrors.General.NotFound, nameof(Doctor), appointment.DoctorId);

        var specialty = await medicalSpecialtyRepository.GetByIdAsync(doctor.MedicalSpecialtyId, cancellationToken) ??
            throw new EntityNotFoundException(DomainErrors.General.NotFound, nameof(MedicalSpecialty), doctor.MedicalSpecialtyId);

        var initiatorDoctor = await doctorRepository.GetByUserIdAsync(initiator.Id, cancellationToken);
        var initiatorPatient = await patientRepository.GetByUserIdAsync(initiator.Id, cancellationToken);

        var context = new AppointmentCancellationContext
        {
            Initiator = initiator,
            InitiatorDoctorId = initiatorDoctor?.Id,
            InitiatorPatientId = initiatorPatient?.Id,
            AppointmentTypeDefinition = appointmentType,
            Specialty = specialty,
            IsAuthorizedFamilyMember = request.IsAuthorizedFamilyMember,
            Reason = request.Reason
        };

        AppointmentCancellationService.CancelAppointment(appointment, context);

        await appointmentRepository.UpdateAsync(appointment, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
