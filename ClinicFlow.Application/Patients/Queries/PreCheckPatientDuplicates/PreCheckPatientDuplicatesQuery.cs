using ClinicFlow.Application.Patients.Queries.DTOs;
using ClinicFlow.Domain.Enums;
using MediatR;

namespace ClinicFlow.Application.Patients.Queries.PreCheckPatientDuplicates;

public sealed record PreCheckPatientDuplicatesQuery(
    Guid ReceptionistUserId,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    BiologicalSex SexAtBirth,
    string AddressLine,
    AddressState AddressState,
    string AddressPostalCode,
    Country AddressCountry,
    string Email,
    string Phone,
    string? SsnLast4
) : IRequest<PatientDuplicatePreCheckDto>;
