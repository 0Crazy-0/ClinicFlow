using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Services;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.Services;

public class PatientPenaltyServiceTests
{
    [Fact]
    public void ApplyPenalty_ShouldReturnWarning_WhenCalled()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var reason = "No show";
        var existingPenalties = new List<PatientPenalty>();

        // Act
        var result = PatientPenaltyService.ApplyPenalty(patientId, existingPenalties, appointmentId, reason).ToList();

        // Assert
        result.Should().ContainSingle();
        var warning = result.First();
        warning.PatientId.Should().Be(patientId);
        warning.Type.Should().Be(PenaltyType.Warning);
        warning.Reason.Should().Be(reason);
    }

    [Fact]
    public void ApplyPenalty_ShouldReturnBlock_WhenStrikesThresholdReached()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        // Simulating 2 existing warnings
        var existingPenalties = new List<PatientPenalty>
        {
            PatientPenalty.CreateWarning(patientId, Guid.NewGuid(), "Warning 1"),
            PatientPenalty.CreateWarning(patientId, Guid.NewGuid(), "Warning 2")
        };

        // Act
        // This call will add the 3rd warning, triggering the block logic
        var result = PatientPenaltyService.ApplyPenalty(patientId, existingPenalties, appointmentId, "Warning 3").ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainSingle(p => p.Type == PenaltyType.Warning);
        result.Should().ContainSingle(p => p.Type == PenaltyType.TemporaryBlock && p.Reason == "Automatic block due to 3 strikes");
    }

    [Fact]
    public void ApplyPenalty_ShouldNotReturnBlock_WhenAlreadyBlocked()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        
        var existingPenalties = new List<PatientPenalty>
        {
            PatientPenalty.CreateWarning(patientId, Guid.NewGuid(), "Warning 1"),
            PatientPenalty.CreateWarning(patientId, Guid.NewGuid(), "Warning 2"),
            PatientPenalty.CreateBlock(patientId, "Existing Block", DateTime.UtcNow.AddDays(10))
        };

        // Act
        var result = PatientPenaltyService.ApplyPenalty(patientId, existingPenalties, Guid.NewGuid(), "Warning 3").ToList();

        // Assert
        result.Should().ContainSingle();
        result.Should().ContainSingle(p => p.Type == PenaltyType.Warning);
    }
}
