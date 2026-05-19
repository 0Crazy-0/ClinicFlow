using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.Doctors.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Doctors.Queries.GetDoctorsBySpecialtyId;

public sealed class GetDoctorsBySpecialtyIdQueryHandler(IDoctorRepository doctorRepository)
    : IRequestHandler<GetDoctorsBySpecialtyIdQuery, PaginatedList<DoctorDto>>
{
    public async Task<PaginatedList<DoctorDto>> Handle(
        GetDoctorsBySpecialtyIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var (items, totalCount) = await doctorRepository.GetBySpecialtyIdPaginatedAsync(
            request.SpecialtyId,
            request.PageNumber,
            request.PageSize,
            cancellationToken
        );

        var dtos = items
            .Select(doctor => new DoctorDto(
                doctor.Id,
                doctor.UserId,
                doctor.MedicalSpecialtyId,
                doctor.LicenseNumber.Value,
                doctor.Biography,
                doctor.ConsultationRoom.Number,
                doctor.ConsultationRoom.Name,
                doctor.ConsultationRoom.Floor
            ))
            .ToList();

        return new PaginatedList<DoctorDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
