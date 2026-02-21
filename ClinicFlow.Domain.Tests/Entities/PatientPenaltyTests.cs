using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.Entities;

public class PatientPenaltyTests
{
    // CreateWarning
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
        act.Should().Throw<BusinessRuleValidationException>().WithMessage("Patient ID cannot be empty.");
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
        act.Should().Throw<BusinessRuleValidationException>().WithMessage("Penalty reason cannot be empty.");
    }

    // CreateBlock
    [Fact]
    public void CreateBlock_ShouldCreateBlockPenalty()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var reason = "Automatic block due to 3 strikes";
        var blockedUntil = DateTime.UtcNow.AddDays(30);

        // Act
        var penalty = PatientPenalty.CreateBlock(patientId, reason, blockedUntil);

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
        var act = () => PatientPenalty.CreateBlock(Guid.Empty, "Block reason", DateTime.UtcNow.AddDays(30));

        // Assert
        act.Should().Throw<BusinessRuleValidationException>().WithMessage("Patient ID cannot be empty.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateBlock_ShouldThrowException_WhenReasonIsEmpty(string? reason)
    {
        // Arrange & Act
        var act = () => PatientPenalty.CreateBlock(Guid.NewGuid(), reason!, DateTime.UtcNow.AddDays(30));

        // Assert
        act.Should().Throw<BusinessRuleValidationException>().WithMessage("Penalty reason cannot be empty.");
    }

    [Fact]
    public void CreateBlock_ShouldThrowException_WhenBlockedUntilIsInThePast()
    {
        // Arrange & Act
        var act = () => PatientPenalty.CreateBlock(Guid.NewGuid(), "Block reason", DateTime.UtcNow.AddDays(-1));

        // Assert
        act.Should().Throw<BusinessRuleValidationException>().WithMessage("Blocked until date must be in the future.");
    }
}
