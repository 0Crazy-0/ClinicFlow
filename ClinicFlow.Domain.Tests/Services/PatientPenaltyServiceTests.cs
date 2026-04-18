using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Services;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services;

public class PatientPenaltyServiceTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void ApplyPenalty_ShouldReturnWarningOnly_WhenFirstOffense()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

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
        result.Should().ContainSingle();
        var warning = result.First();
        warning.Type.Should().Be(PenaltyType.Warning);
        warning.Reason.Should().Be(PenaltyReasons.NoShow);
    }

    [Fact]
    public void ApplyPenalty_ShouldReturnMinorBlock_WhenSecondOffense()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var existingPenalties = new List<PatientPenalty>
        {
            PatientPenalty.CreateAutomaticWarning(patientId, Guid.NewGuid(), "Warning 1"),
        };

        // Act
        var result = PatientPenaltyService
            .ApplyPenalty(
                patientId,
                existingPenalties,
                Guid.NewGuid(),
                "Warning 2",
                _fakeTime.GetUtcNow().UtcDateTime
            )
            .ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainSingle(p => p.Type == PenaltyType.Warning);

        var block = result.Single(p => p.Type == PenaltyType.TemporaryBlock);
        block.Reason.Should().Be(PenaltyReasons.AutomaticBlock);
        block
            .BlockedUntil.Should()
            .Be(_fakeTime.GetUtcNow().UtcDateTime.Date.AddDays((int)BlockDuration.Minor));
    }

    [Fact]
    public void ApplyPenalty_ShouldReturnModerateBlock_WhenThirdOffense()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var existingPenalties = new List<PatientPenalty>
        {
            PatientPenalty.CreateAutomaticWarning(patientId, Guid.NewGuid(), "Warning 1"),
            PatientPenalty.CreateAutomaticWarning(patientId, Guid.NewGuid(), "Warning 2"),
            PatientPenalty.CreateAutomaticBlock(
                patientId,
                PenaltyReasons.AutomaticBlock,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-1).Date,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-6).Date
            ),
        };

        // Act
        var result = PatientPenaltyService
            .ApplyPenalty(
                patientId,
                existingPenalties,
                Guid.NewGuid(),
                "Warning 3",
                _fakeTime.GetUtcNow().UtcDateTime
            )
            .ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainSingle(p => p.Type == PenaltyType.Warning);

        var block = result.Single(p => p.Type == PenaltyType.TemporaryBlock);
        block
            .BlockedUntil.Should()
            .Be(_fakeTime.GetUtcNow().UtcDateTime.Date.AddDays((int)BlockDuration.Moderate));
    }

    [Fact]
    public void ApplyPenalty_ShouldReturnSevereBlock_WhenFourthOffenseOrMore()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var existingPenalties = new List<PatientPenalty>
        {
            PatientPenalty.CreateAutomaticWarning(patientId, Guid.NewGuid(), "Warning 1"),
            PatientPenalty.CreateAutomaticWarning(patientId, Guid.NewGuid(), "Warning 2"),
            PatientPenalty.CreateAutomaticBlock(
                patientId,
                PenaltyReasons.AutomaticBlock,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-20).Date,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-25).Date
            ),
            PatientPenalty.CreateAutomaticWarning(patientId, Guid.NewGuid(), "Warning 3"),
            PatientPenalty.CreateAutomaticBlock(
                patientId,
                PenaltyReasons.AutomaticBlock,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-1).Date,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-16).Date
            ),
        };

        // Act
        var result = PatientPenaltyService
            .ApplyPenalty(
                patientId,
                existingPenalties,
                Guid.NewGuid(),
                "Warning 4",
                _fakeTime.GetUtcNow().UtcDateTime
            )
            .ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainSingle(p => p.Type == PenaltyType.Warning);

        var block = result.Single(p => p.Type == PenaltyType.TemporaryBlock);
        block
            .BlockedUntil.Should()
            .Be(_fakeTime.GetUtcNow().UtcDateTime.Date.AddDays((int)BlockDuration.Severe));
    }

    [Fact]
    public void ApplyPenalty_ShouldNotReturnBlock_WhenAlreadyBlocked()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var existingPenalties = new List<PatientPenalty>
        {
            PatientPenalty.CreateAutomaticWarning(patientId, Guid.NewGuid(), "Warning 1"),
            PatientPenalty.CreateAutomaticBlock(
                patientId,
                PenaltyReasons.AutomaticBlock,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(10).Date,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
        };

        // Act
        var result = PatientPenaltyService
            .ApplyPenalty(
                patientId,
                existingPenalties,
                Guid.NewGuid(),
                "Warning 2",
                _fakeTime.GetUtcNow().UtcDateTime
            )
            .ToList();

        // Assert
        result.Should().ContainSingle();
        result.Should().ContainSingle(p => p.Type == PenaltyType.Warning);
    }
}
