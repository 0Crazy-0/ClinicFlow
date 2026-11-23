using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services.Contexts.Registration;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides the repository implementation for <see cref="Patient"/> persistence operations.
/// </summary>
public sealed class PatientRepository(ApplicationDbContext dbContext) : IPatientRepository
{
    private const float MinimumSimilarity = 0.30f;
    private const int CandidateLimit = 20;

    public Task CreateAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        if (dbContext.Entry(patient).State is EntityState.Detached)
            dbContext.Patients.Add(patient);

        return Task.CompletedTask;
    }

    public async Task<Patient?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    ) => await dbContext.Patients.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PatientDuplicateCandidate>> GetFuzzyMatchesBySearchNameAsync(
        string searchName,
        DateOnly dateOfBirth,
        string? ssnLast4Hash,
        CancellationToken cancellationToken = default
    )
    {
        var matches = await dbContext
            .Patients.AsNoTracking()
            .Where(p =>
                p.DateOfBirth == dateOfBirth
                && EF.Functions.TrigramsSimilarity(p.FullName.SearchName, searchName)
                    >= MinimumSimilarity
            )
            .OrderByDescending(p =>
                EF.Functions.TrigramsSimilarity(p.FullName.SearchName, searchName)
            )
            .Take(CandidateLimit)
            .Select(patient => new
            {
                Patient = patient,
                Contact = dbContext
                    .Users.Where(u =>
                        dbContext.FamilyMemberships.Any(m =>
                            m.UserId == u.Id
                            && m.PatientId == patient.Id
                            && m.Role == PatientRelationship.Self
                            && m.Status == FamilyMembershipStatus.Active
                        )
                    )
                    .Select(u => new { u.Email, u.Phone })
                    .FirstOrDefault(),
                SsnLast4Matches = ssnLast4Hash != null && patient.SsnLast4Hash == ssnLast4Hash,
            })
            .ToListAsync(cancellationToken);

        return
        [
            .. matches.Select(m => new PatientDuplicateCandidate
            {
                PatientId = m.Patient.Id,
                FullName = m.Patient.FullName,
                DateOfBirth = m.Patient.DateOfBirth,
                SexAtBirth = m.Patient.SexAtBirth,
                HomeAddress = m.Patient.HomeAddress,
                Email = m.Contact?.Email,
                Phone = m.Contact?.Phone,
                SsnLast4Matches = m.SsnLast4Matches,
            }),
        ];
    }
}
