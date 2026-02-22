using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Services;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointment;

public class CancelAppointmentCommandHandler(AppointmentCancellationService cancellationService, IUnitOfWork unitOfWork) : IRequestHandler<CancelAppointmentCommand>
{
    public async Task Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        await cancellationService.CancelAppointmentAsync(request.AppointmentId, request.InitiatorUserId, request.IsAuthorizedFamilyMember, request.Reason);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
