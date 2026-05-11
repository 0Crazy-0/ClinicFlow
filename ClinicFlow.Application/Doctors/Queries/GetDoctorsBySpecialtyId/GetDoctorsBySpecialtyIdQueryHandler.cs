using ClinicFlow.Application.Doctors.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Doctors.Queries.GetDoctorsBySpecialtyId;

public sealed class GetDoctorsBySpecialtyIdQueryHandler(IDoctorRepository doctorRepository)
    : IRequestHandler<GetDoctorsBySpecialtyIdQuery, IReadOnlyList<DoctorDto>>
{
    public async Task<IReadOnlyList<DoctorDto>> Handle(
        GetDoctorsBySpecialtyIdQuery request,
        CancellationToken ct
    )
    {
        var doctors = await doctorRepository.GetBySpecialtyIdAsync(request.SpecialtyId, ct);

        return
        [
            .. doctors.Select(doctor => new DoctorDto(
                doctor.Id,
                doctor.UserId,
                doctor.MedicalSpecialtyId,
                doctor.LicenseNumber.Value,
                doctor.Biography,
                doctor.ConsultationRoom.Number,
                doctor.ConsultationRoom.Name,
                doctor.ConsultationRoom.Floor
            )),
        ];
    }
}
