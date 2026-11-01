using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Events.Users;
using ClinicFlow.Domain.Exceptions.Base;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Entities;

public class PatientActivationCodeTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void Issue_ShouldCreateCode_WithHashIssuerAndSevenDayWindow()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var codeHash = "hash";
        var issuedByUserId = Guid.CreateVersion7();
        var issuedAt = _fakeTime.GetLocalNow().DateTime;

        // Act
        var code = PatientActivationCode.Issue(userId, codeHash, issuedByUserId, issuedAt);

        // Assert
        code.UserId.Should().Be(userId);
        code.CodeHash.Should().Be(codeHash);
        code.IssuedByUserId.Should().Be(issuedByUserId);
        code.IssuedAt.Should().Be(issuedAt);
        code.ExpiresAt.Should().Be(issuedAt.AddDays(7));
        code.FailedAttempts.Should().Be(0);
        code.ConsumedAt.Should().BeNull();
        code.SupersededAt.Should().BeNull();

        var issuedEvent = code
            .DomainEvents.OfType<PatientActivationCodeIssuedEvent>()
            .Should()
            .ContainSingle()
            .Which;

        issuedEvent.UserId.Should().Be(userId);
        issuedEvent.IssuedByUserId.Should().Be(issuedByUserId);
        issuedEvent.IssuedAt.Should().Be(issuedAt);
        issuedEvent.ExpiresAt.Should().Be(issuedAt.AddDays(7));
    }

    [Fact]
    public void Issue_ShouldThrowDomainValidationException_WhenUserIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            PatientActivationCode.Issue(
                Guid.Empty,
                "hash",
                Guid.CreateVersion7(),
                _fakeTime.GetLocalNow().DateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Issue_ShouldThrowDomainValidationException_WhenCodeHashIsNullOrWhiteSpace(
        string? codeHash
    )
    {
        // Arrange & Act
        var act = () =>
            PatientActivationCode.Issue(
                Guid.CreateVersion7(),
                codeHash!,
                Guid.CreateVersion7(),
                _fakeTime.GetLocalNow().DateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Issue_ShouldThrowDomainValidationException_WhenIssuedByUserIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            PatientActivationCode.Issue(
                Guid.CreateVersion7(),
                "hash",
                Guid.Empty,
                _fakeTime.GetLocalNow().DateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Issue_ShouldThrowDomainValidationException_WhenIssuedAtIsDefault()
    {
        // Arrange & Act
        var act = () =>
            PatientActivationCode.Issue(
                Guid.CreateVersion7(),
                "hash",
                Guid.CreateVersion7(),
                default
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void IsValid_ShouldThrowDomainValidationException_WhenReferenceTimeIsDefault()
    {
        // Arrange
        var issuedAt = _fakeTime.GetLocalNow().DateTime;
        var code = CreateIssuedCode(issuedAt);

        // Act
        var act = () => code.IsValid(default);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenFreshlyIssued()
    {
        // Arrange
        var issuedAt = _fakeTime.GetLocalNow().DateTime;
        var code = CreateIssuedCode(issuedAt);
        var referenceTime = issuedAt.AddDays(3);

        // Act
        var isValid = code.IsValid(referenceTime);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenReferenceTimeEqualsIssuedAt()
    {
        // Arrange
        var issuedAt = _fakeTime.GetLocalNow().DateTime;
        var code = CreateIssuedCode(issuedAt);

        // Act
        var isValid = code.IsValid(issuedAt);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenReferenceTimeIsBeforeIssuedAt()
    {
        // Arrange
        var issuedAt = _fakeTime.GetLocalNow().DateTime;
        var code = CreateIssuedCode(issuedAt);
        var referenceTime = issuedAt.AddSeconds(-1);

        // Act
        var isValid = code.IsValid(referenceTime);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenExpired()
    {
        // Arrange
        var issuedAt = _fakeTime.GetLocalNow().DateTime;
        var code = CreateIssuedCode(issuedAt);
        var referenceTime = issuedAt.AddDays(7).AddSeconds(1);

        // Act
        var isValid = code.IsValid(referenceTime);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenExpiresAtEqualsReferenceTime()
    {
        // Arrange
        var issuedAt = _fakeTime.GetLocalNow().DateTime;
        var code = CreateIssuedCode(issuedAt);
        var referenceTime = issuedAt.AddDays(7);

        // Act
        var isValid = code.IsValid(referenceTime);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenConsumed()
    {
        // Arrange
        var issuedAt = _fakeTime.GetLocalNow().DateTime;
        var code = CreateIssuedCode(issuedAt);

        var consumedAt = issuedAt.AddDays(2);

        code.Consume(consumedAt);

        // Act
        var isValid = code.IsValid(issuedAt.AddDays(3));

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenSuperseded()
    {
        // Arrange
        var issuedAt = _fakeTime.GetLocalNow().DateTime;
        var code = CreateIssuedCode(issuedAt);

        code.Supersede(issuedAt.AddDays(1));

        // Act
        var isValid = code.IsValid(issuedAt.AddDays(3));

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenFailedAttemptsReachedMax()
    {
        // Arrange
        var issuedAt = _fakeTime.GetLocalNow().DateTime;
        var code = CreateIssuedCode(issuedAt);

        code.RecordFailedAttempt();
        code.RecordFailedAttempt();
        code.RecordFailedAttempt();
        code.RecordFailedAttempt();
        code.RecordFailedAttempt();

        // Act
        var isValid = code.IsValid(issuedAt.AddDays(1));

        // Assert
        isValid.Should().BeFalse();

        code.FailedAttempts.Should().Be(5);
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenFailedAttemptsAreApproachingMax()
    {
        // Arrange
        var issuedAt = _fakeTime.GetLocalNow().DateTime;
        var code = CreateIssuedCode(issuedAt);

        code.RecordFailedAttempt();
        code.RecordFailedAttempt();
        code.RecordFailedAttempt();
        code.RecordFailedAttempt();

        // Act
        var isValid = code.IsValid(issuedAt.AddDays(1));

        // Assert
        isValid.Should().BeTrue();

        code.FailedAttempts.Should().Be(4);
    }

    [Fact]
    public void Consume_ShouldSetConsumedAt()
    {
        // Arrange
        var issuedAt = _fakeTime.GetLocalNow().DateTime;
        var code = CreateIssuedCode(issuedAt);
        var referenceTime = issuedAt.AddDays(2);

        // Act
        code.Consume(referenceTime);

        // Assert
        code.ConsumedAt.Should().Be(referenceTime);
    }

    [Fact]
    public void Supersede_ShouldSetSupersededAt()
    {
        // Arrange
        var issuedAt = _fakeTime.GetLocalNow().DateTime;
        var code = CreateIssuedCode(issuedAt);
        var referenceTime = issuedAt.AddDays(1);

        // Act
        code.Supersede(referenceTime);

        // Assert
        code.SupersededAt.Should().Be(referenceTime);
    }

    [Fact]
    public void Consume_ShouldThrowDomainValidationException_WhenReferenceTimeIsDefault()
    {
        // Arrange
        var issuedAt = _fakeTime.GetLocalNow().DateTime;
        var code = CreateIssuedCode(issuedAt);

        // Act
        var act = () => code.Consume(default);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Supersede_ShouldThrowDomainValidationException_WhenReferenceTimeIsDefault()
    {
        // Arrange
        var issuedAt = _fakeTime.GetLocalNow().DateTime;
        var code = CreateIssuedCode(issuedAt);

        // Act
        var act = () => code.Supersede(default);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    private static PatientActivationCode CreateIssuedCode(DateTime issuedAt) =>
        PatientActivationCode.Issue(Guid.CreateVersion7(), "hash", Guid.CreateVersion7(), issuedAt);
}
