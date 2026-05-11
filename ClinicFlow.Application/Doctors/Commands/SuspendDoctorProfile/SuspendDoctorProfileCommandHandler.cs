using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Doctors.Commands.SuspendDoctorProfile;

public sealed class SuspendDoctorProfileCommandHandler(
    IDoctorRepository doctorRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<SuspendDoctorProfileCommand>
{
    public async Task Handle(SuspendDoctorProfileCommand request, CancellationToken ct)
    {
        var doctor =
            await doctorRepository.GetByIdAsync(request.DoctorId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Doctor),
                request.DoctorId
            );

        doctor.Suspend();

        await unitOfWork.SaveChangesAsync(ct);
    }
}
