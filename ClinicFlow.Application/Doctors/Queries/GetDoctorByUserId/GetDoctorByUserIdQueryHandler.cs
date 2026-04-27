using ClinicFlow.Application.Doctors.Queries.DTOs;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Doctors.Queries.GetDoctorByUserId;

public sealed class GetDoctorByUserIdQueryHandler(IDoctorRepository doctorRepository)
    : IRequestHandler<GetDoctorByUserIdQuery, DoctorDto>
{
    public async Task<DoctorDto> Handle(
        GetDoctorByUserIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var doctor =
            await doctorRepository.GetByUserIdAsync(request.UserId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Doctor),
                request.UserId
            );

        return new DoctorDto(
            doctor.Id,
            doctor.UserId,
            doctor.MedicalSpecialtyId,
            doctor.LicenseNumber.Value,
            doctor.Biography,
            doctor.ConsultationRoom.Number,
            doctor.ConsultationRoom.Name,
            doctor.ConsultationRoom.Floor
        );
    }
}
