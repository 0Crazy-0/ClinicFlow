using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Doctors.Commands.CreateDoctorProfile;

public sealed class CreateDoctorProfileCommandHandler(
    IDoctorRepository doctorRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateDoctorProfileCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Guid> Handle(
        CreateDoctorProfileCommand request,
        CancellationToken cancellationToken
    )
    {
        var existingDoctor = await doctorRepository.GetIncludingDeletedByLicenseNumberAsync(
            request.LicenseNumber,
            cancellationToken
        );

        var fullName = PersonName.Create($"{request.FirstName} {request.LastName}");
        var args = new DoctorRegistrationArgs
        {
            UserId = request.UserId,
            FullName = fullName,
            LicenseNumber = MedicalLicenseNumber.Create(request.LicenseNumber),
            MedicalSpecialtyId = request.MedicalSpecialtyId,
            Biography = request.Biography,
            ConsultationRoom = ConsultationRoom.Create(
                request.ConsultationRoomNumber,
                request.ConsultationRoomName,
                request.ConsultationRoomFloor
            ),
        };

        var doctor = DoctorRegistrationService.Register(args, existingDoctor);

        await doctorRepository.CreateAsync(doctor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return doctor.Id;
    }
}
