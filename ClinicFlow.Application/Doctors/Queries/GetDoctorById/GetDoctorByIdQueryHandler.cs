using ClinicFlow.Application.Doctors.Queries.DTOs;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Doctors.Queries.GetDoctorById;

public sealed class GetDoctorByIdQueryHandler(IDoctorRepository doctorRepository)
    : IRequestHandler<GetDoctorByIdQuery, DoctorDto>
{
    public async Task<DoctorDto> Handle(
        GetDoctorByIdQuery request,
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
