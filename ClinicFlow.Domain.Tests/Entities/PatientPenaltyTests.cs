using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Entities;

public class PatientPenaltyTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void CreateWarning_ShouldCreateWarningPenalty()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var reason = "No show";

        // Act
        var penalty = PatientPenalty.CreateWarning(patientId, appointmentId, reason);

        // Assert
        penalty.Should().NotBeNull();
        penalty.PatientId.Should().Be(patientId);
        penalty.AppointmentId.Should().Be(appointmentId);
        penalty.Type.Should().Be(PenaltyType.Warning);
        penalty.Reason.Should().Be(reason);
        penalty.BlockedUntil.Should().BeNull();
    }

    [Fact]
    public void CreateWarning_ShouldThrowException_WhenPatientIdIsEmpty()
    {
        // Arrange & Act
        var act = () => PatientPenalty.CreateWarning(Guid.Empty, Guid.NewGuid(), "No show");

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateWarning_ShouldThrowException_WhenReasonIsEmpty(string? reason)
    {
        // Arrange & Act
        var act = () => PatientPenalty.CreateWarning(Guid.NewGuid(), Guid.NewGuid(), reason!);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void CreateBlock_ShouldCreateBlockPenalty()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var reason = "Automatic block due to 3 strikes";
        var blockedUntil = _fakeTime.GetUtcNow().UtcDateTime.AddDays(30).Date;

        // Act
        var penalty = PatientPenalty.CreateBlock(
            patientId,
            reason,
            blockedUntil,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Assert
        penalty.Should().NotBeNull();
        penalty.PatientId.Should().Be(patientId);
        penalty.AppointmentId.Should().BeNull();
        penalty.Type.Should().Be(PenaltyType.TemporaryBlock);
        penalty.Reason.Should().Be(reason);
        penalty.BlockedUntil.Should().Be(blockedUntil);
    }

    [Fact]
    public void CreateBlock_ShouldThrowException_WhenPatientIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            PatientPenalty.CreateBlock(
                Guid.Empty,
                "Block reason",
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(30).Date,
                _fakeTime.GetUtcNow().UtcDateTime
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
    public void CreateBlock_ShouldThrowException_WhenReasonIsEmpty(string? reason)
    {
        // Arrange & Act
        var act = () =>
            PatientPenalty.CreateBlock(
                Guid.NewGuid(),
                reason!,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(30).Date,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void CreateBlock_ShouldThrowException_WhenBlockedUntilIsInThePast()
    {
        // Arrange & Act
        var act = () =>
            PatientPenalty.CreateBlock(
                Guid.NewGuid(),
                "Block reason",
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-1).Date,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueMustBeInFuture);
    }
}
