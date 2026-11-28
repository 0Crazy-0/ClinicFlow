using ClinicFlow.Application.Interfaces;
using ClinicFlow.Application.Patients.Queries.DTOs;
using ClinicFlow.Application.Patients.Queries.Shared;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.Services.Contexts.Registration;
using ClinicFlow.Domain.Services.Registration;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Patients.Queries.PreCheckMinorPatientDuplicates;

public sealed class PreCheckMinorPatientDuplicatesQueryHandler(
    IPatientRepository patientRepository,
    ISsnHasher ssnHasher
) : IRequestHandler<PreCheckMinorPatientDuplicatesQuery, PatientDuplicatePreCheckDto>
{
    /// <inheritdoc />
    public async Task<PatientDuplicatePreCheckDto> Handle(
        PreCheckMinorPatientDuplicatesQuery request,
        CancellationToken cancellationToken
    )
    {
        var fullName = PersonName.Create(request.FirstName, null, request.LastName);
        var homeAddress = Address.Create(
            request.AddressLine,
            request.AddressState,
            request.AddressPostalCode,
            request.AddressCountry
        );
        var candidates = await patientRepository.GetFuzzyMatchesBySearchNameAsync(
            fullName.SearchName,
            request.DateOfBirth,
            cancellationToken
        );

        var candidatesWithSsnMatches = candidates
            .Select(candidate =>
                candidate with
                {
                    SsnLast4Matches =
                        request.SsnLast4 is not null
                        && ssnHasher.ComputeHash(candidate.PatientId, request.SsnLast4)
                            == candidate.SsnLast4Hash,
                }
            )
            .ToList();

        var result = MinorPatientDuplicateScoringService.Score(
            new MinorPatientDuplicateScoringArgs
            {
                FirstName = request.FirstName,
                FirstSurname = request.LastName,
                DateOfBirth = request.DateOfBirth,
                SexAtBirth = request.SexAtBirth,
                HomeAddress = homeAddress,
            },
            new PatientDuplicateScoringContext { Candidates = candidatesWithSsnMatches }
        );

        var scores = result
            .Scores.Select(score => new PatientDuplicatePreCheckScoreDto(
                score.Candidate.PatientId,
                score.Candidate.FullName.ToString(),
                score.Candidate.DateOfBirth,
                score.Candidate.SexAtBirth,
                score.Candidate.HomeAddress.Line,
                score.Candidate.HomeAddress.State,
                score.Candidate.HomeAddress.PostalCode,
                MatchMasking.MaskEmail(score.Candidate.Email?.ToString()),
                MatchMasking.MaskPhone(score.Candidate.Phone?.ToString()),
                score.Score
            ))
            .ToList();

        return new PatientDuplicatePreCheckDto(scores, result.MaxScore, result.IsHighBand);
    }
}
