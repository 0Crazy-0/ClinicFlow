using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Services;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services;

public class PatientPenaltyServiceTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void ApplyPenalty_ShouldReturnWarningOnly_WhenFirstOffense()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var appointmentId = Guid.CreateVersion7();

        // Act
        var result = PatientPenaltyService
            .ApplyPenalty(
                patientId,
                [],
                appointmentId,
                "No show",
                _fakeTime.GetUtcNow().UtcDateTime
            )
            .ToList();

        // Assert
        var warning = result.Should().ContainSingle().Subject;
        warning.Type.Should().Be(PenaltyType.Warning);
        warning.Reason.Should().Be(PenaltyReasons.NoShow);
    }

    [Fact]
    public void ApplyPenalty_ShouldReturnMinorBlock_WhenSecondOffense()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var existingPenalties = new List<PatientPenalty>
        {
            PatientPenalty.CreateAutomaticWarning(patientId, Guid.CreateVersion7(), "Warning 1"),
        };

        var now = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        var result = PatientPenaltyService
            .ApplyPenalty(patientId, existingPenalties, Guid.CreateVersion7(), "Warning 2", now)
            .ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainSingle(p => p.Type == PenaltyType.Warning);

        var expectedBlockedUntil = DateOnly.FromDateTime(now).AddDays((int)BlockDuration.Minor);

        result
            .Should()
            .ContainSingle(p => p.Type == PenaltyType.TemporaryBlock)
            .Which.Should()
            .Match<PatientPenalty>(block => block.BlockedUntil == expectedBlockedUntil);
    }

    [Fact]
    public void ApplyPenalty_ShouldReturnModerateBlock_WhenThirdOffense()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var now = _fakeTime.GetUtcNow().UtcDateTime;
        var existingPenalties = new List<PatientPenalty>
        {
            PatientPenalty.CreateAutomaticWarning(patientId, Guid.CreateVersion7(), "Warning 1"),
            PatientPenalty.CreateAutomaticWarning(patientId, Guid.CreateVersion7(), "Warning 2"),
            PatientPenalty.CreateAutomaticBlock(
                patientId,
                PenaltyReasons.AutomaticBlock,
                BlockDuration.Minor,
                now.AddDays(-6)
            ),
        };

        // Act
        var result = PatientPenaltyService
            .ApplyPenalty(patientId, existingPenalties, Guid.CreateVersion7(), "Warning 3", now)
            .ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainSingle(p => p.Type == PenaltyType.Warning);

        var expectedBlockedUntil = DateOnly.FromDateTime(now).AddDays((int)BlockDuration.Moderate);

        result
            .Should()
            .ContainSingle(p => p.Type == PenaltyType.TemporaryBlock)
            .Which.Should()
            .Match<PatientPenalty>(block => block.BlockedUntil == expectedBlockedUntil);
    }

    [Fact]
    public void ApplyPenalty_ShouldReturnSevereBlock_WhenFourthOffenseOrMore()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var now = _fakeTime.GetUtcNow().UtcDateTime;
        var existingPenalties = new List<PatientPenalty>
        {
            PatientPenalty.CreateAutomaticWarning(patientId, Guid.CreateVersion7(), "Warning 1"),
            PatientPenalty.CreateAutomaticWarning(patientId, Guid.CreateVersion7(), "Warning 2"),
            PatientPenalty.CreateAutomaticBlock(
                patientId,
                PenaltyReasons.AutomaticBlock,
                BlockDuration.Minor,
                now.AddDays(-25)
            ),
            PatientPenalty.CreateAutomaticWarning(patientId, Guid.CreateVersion7(), "Warning 3"),
            PatientPenalty.CreateAutomaticBlock(
                patientId,
                PenaltyReasons.AutomaticBlock,
                BlockDuration.Minor,
                now.AddDays(-6)
            ),
        };

        // Act
        var result = PatientPenaltyService
            .ApplyPenalty(patientId, existingPenalties, Guid.CreateVersion7(), "Warning 4", now)
            .ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainSingle(p => p.Type == PenaltyType.Warning);

        var expectedBlockedUntil = DateOnly.FromDateTime(now).AddDays((int)BlockDuration.Severe);

        result
            .Should()
            .ContainSingle(p => p.Type == PenaltyType.TemporaryBlock)
            .Which.Should()
            .Match<PatientPenalty>(block => block.BlockedUntil == expectedBlockedUntil);
    }

    [Fact]
    public void ApplyPenalty_ShouldNotReturnBlock_WhenAlreadyBlocked()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var existingPenalties = new List<PatientPenalty>
        {
            PatientPenalty.CreateAutomaticWarning(patientId, Guid.CreateVersion7(), "Warning 1"),
            PatientPenalty.CreateAutomaticBlock(
                patientId,
                PenaltyReasons.AutomaticBlock,
                BlockDuration.Minor,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
        };

        // Act
        var result = PatientPenaltyService
            .ApplyPenalty(
                patientId,
                existingPenalties,
                Guid.CreateVersion7(),
                "Warning 2",
                _fakeTime.GetUtcNow().UtcDateTime
            )
            .ToList();

        // Assert
        result.Should().ContainSingle(p => p.Type == PenaltyType.Warning);
        result.Should().NotContain(p => p.Type == PenaltyType.TemporaryBlock);
    }
}
