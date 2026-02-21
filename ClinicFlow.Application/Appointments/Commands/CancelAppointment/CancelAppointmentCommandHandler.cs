using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Services;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointment;

public class CancelAppointmentCommandHandler(IAppointmentRepository appointmentRepository, IUserRepository userRepository,
    IAppointmentTypeDefinitionRepository appointmentTypeDefinitionRepository, AppointmentCancellationService cancellationService, IUnitOfWork unitOfWork) : IRequestHandler<CancelAppointmentCommand>
{
    public async Task Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdAsync(request.AppointmentId) ?? throw new EntityNotFoundException(nameof(Appointment), request.AppointmentId);

        var initiator = await userRepository.GetByIdAsync(request.InitiatorUserId) ?? throw new EntityNotFoundException(nameof(User), request.InitiatorUserId);

        var appointmentType = await appointmentTypeDefinitionRepository.GetByIdAsync(appointment.AppointmentTypeId)
            ?? throw new EntityNotFoundException(nameof(AppointmentTypeDefinition), appointment.AppointmentTypeId);

        await cancellationService.CancelAppointmentAsync(appointment, initiator, appointmentType, request.IsAuthorizedFamilyMember, request.Reason);

        await appointmentRepository.UpdateAsync(appointment);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
