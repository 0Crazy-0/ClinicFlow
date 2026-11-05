using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Doctors.Commands.ReactivateDoctorProfile;

public sealed class ReactivateDoctorProfileCommandHandler(
    IDoctorRepository doctorRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ReactivateDoctorProfileCommand>
{
    /// <inheritdoc />
    public async Task Handle(
        ReactivateDoctorProfileCommand request,
        CancellationToken cancellationToken
    )
    {
        var doctor =
            await doctorRepository.GetByIdAsync(request.DoctorId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Doctor),
                request.DoctorId
            );

        doctor.Reactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
