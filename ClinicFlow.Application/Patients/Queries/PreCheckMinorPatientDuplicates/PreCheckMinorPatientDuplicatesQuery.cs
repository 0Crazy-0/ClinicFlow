using ClinicFlow.Application.Patients.Queries.DTOs;
using ClinicFlow.Domain.Enums;
using MediatR;

namespace ClinicFlow.Application.Patients.Queries.PreCheckMinorPatientDuplicates;

public sealed record PreCheckMinorPatientDuplicatesQuery(
    Guid ReceptionistUserId,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    BiologicalSex SexAtBirth,
    string AddressLine,
    AddressState AddressState,
    string AddressPostalCode,
    Country AddressCountry,
    string? SsnLast4
) : IRequest<PatientDuplicatePreCheckDto>;
