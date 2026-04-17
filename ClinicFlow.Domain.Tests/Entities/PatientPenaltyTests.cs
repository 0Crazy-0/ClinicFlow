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
    public void CreateAutomaticWarning_ShouldCreateWarningPenalty()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var reason = "No show";

        // Act
        var penalty = PatientPenalty.CreateAutomaticWarning(patientId, appointmentId, reason);

        // Assert
        penalty.Should().NotBeNull();
        penalty.PatientId.Should().Be(patientId);
        penalty.AppointmentId.Should().Be(appointmentId);
        penalty.Type.Should().Be(PenaltyType.Warning);
        penalty.Reason.Should().Be(reason);
        penalty.BlockedUntil.Should().BeNull();
    }

    [Fact]
    public void CreateAutomaticWarning_ShouldThrowException_WhenPatientIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            PatientPenalty.CreateAutomaticWarning(Guid.Empty, Guid.NewGuid(), "No show");

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateAutomaticWarning_ShouldThrowException_WhenReasonIsEmpty(string? reason)
    {
        // Arrange & Act
        var act = () =>
            PatientPenalty.CreateAutomaticWarning(Guid.NewGuid(), Guid.NewGuid(), reason!);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void CreateAutomaticBlock_ShouldCreateBlockPenalty()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var reason = "Automatic block due to 3 strikes";
        var blockedUntil = _fakeTime.GetUtcNow().UtcDateTime.AddDays(30).Date;

        // Act
        var penalty = PatientPenalty.CreateAutomaticBlock(
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
    public void CreateAutomaticBlock_ShouldThrowException_WhenPatientIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            PatientPenalty.CreateAutomaticBlock(
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
    public void CreateAutomaticBlock_ShouldThrowException_WhenReasonIsEmpty(string? reason)
    {
        // Arrange & Act
        var act = () =>
            PatientPenalty.CreateAutomaticBlock(
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
    public void CreateAutomaticBlock_ShouldThrowException_WhenBlockedUntilIsInThePast()
    {
        // Arrange & Act
        var act = () =>
            PatientPenalty.CreateAutomaticBlock(
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

    [Fact]
    public void Remove_ShouldSetIsRemovedToTrue()
    {
        // Arrange
        var penalty = PatientPenalty.CreateAutomaticBlock(
            Guid.NewGuid(),
            "Block reason",
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(30).Date,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        penalty.Remove();

        // Assert
        penalty.IsRemoved.Should().BeTrue();
    }

    [Fact]
    public void Remove_ShouldThrowException_WhenAlreadyRemoved()
    {
        // Arrange
        var penalty = PatientPenalty.CreateAutomaticBlock(
            Guid.NewGuid(),
            "Block reason",
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(30).Date,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        penalty.Remove();

        // Act & Assert
        penalty
            .Invoking(p => p.Remove())
            .Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Penalty.AlreadyRemoved);
    }
}
