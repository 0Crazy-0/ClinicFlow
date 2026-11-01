using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Events.Users;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Represents a one-time activation code issued to a pending patient user account.
/// </summary>
/// <remarks>
/// The code is stored hashed; repeated failed verification attempts (see <see cref="MaxFailedAttempts"/>)
/// and consumption or supersession permanently invalidate the code.
/// </remarks>
public class PatientActivationCode : BaseEntity
{
    public const int MaxFailedAttempts = 5;
    public const int ValidityDays = 7;

    public Guid UserId { get; private set; }

    public string CodeHash { get; private set; } = null!;

    public Guid IssuedByUserId { get; private set; }

    public DateTime IssuedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public DateTime? ConsumedAt { get; private set; }

    public DateTime? SupersededAt { get; private set; }

    public int FailedAttempts { get; private set; }

    // EF Core constructor
    private PatientActivationCode() { }

    private PatientActivationCode(
        Guid userId,
        string codeHash,
        Guid issuedByUserId,
        DateTime issuedAt,
        DateTime expiresAt
    )
    {
        UserId = userId;
        CodeHash = codeHash;
        IssuedByUserId = issuedByUserId;
        IssuedAt = issuedAt;
        ExpiresAt = expiresAt;
    }

    /// <summary>
    /// Issues a new activation code for the given pending user, valid for <see cref="ValidityDays"/> days.
    /// </summary>
    public static PatientActivationCode Issue(
        Guid userId,
        string codeHash,
        Guid issuedByUserId,
        DateTime issuedAt
    )
    {
        Guard.NotEmpty(userId);
        Guard.NotNullOrWhiteSpace(codeHash);
        Guard.NotEmpty(issuedByUserId);
        Guard.NotDefault(issuedAt);

        var code = new PatientActivationCode(
            userId,
            codeHash,
            issuedByUserId,
            issuedAt,
            issuedAt.AddDays(ValidityDays)
        );

        code.AddDomainEvent(
            new PatientActivationCodeIssuedEvent(userId, issuedByUserId, issuedAt, code.ExpiresAt)
        );

        return code;
    }

    public bool IsValid(DateTime referenceTime)
    {
        Guard.NotDefault(referenceTime);

        return ConsumedAt is null
            && SupersededAt is null
            && IssuedAt <= referenceTime
            && ExpiresAt > referenceTime
            && FailedAttempts < MaxFailedAttempts;
    }

    public void RecordFailedAttempt() => FailedAttempts++;

    /// <summary>
    /// Marks the code as used by a successful activation without checking its validity state.
    /// </summary>
    /// <remarks>
    /// Callers must verify <see cref="IsValid"/> before calling this method,
    /// otherwise an expired, superseded, or exhausted code can be activated.
    /// </remarks>
    public void Consume(DateTime referenceTime)
    {
        Guard.NotDefault(referenceTime);

        ConsumedAt = referenceTime;
    }

    /// <summary>
    /// Marks the code as replaced by a newer code.
    /// </summary>
    public void Supersede(DateTime referenceTime)
    {
        Guard.NotDefault(referenceTime);

        SupersededAt = referenceTime;
    }
}
