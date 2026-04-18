using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.ValueObjects;

public class PenaltyHistoryTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void HasPriorWarnings_ShouldReturnFalse_WhenHistoryIsEmpty()
    {
        // Arrange
        var history = new PenaltyHistory([]);

        // Assert
        history.HasPriorWarnings.Should().BeFalse();
    }

    [Fact]
    public void HasPriorWarnings_ShouldReturnTrue_WhenWarningsExist()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var history = new PenaltyHistory([
            PatientPenalty.CreateAutomaticWarning(patientId, Guid.NewGuid(), "No show"),
        ]);

        // Assert
        history.HasPriorWarnings.Should().BeTrue();
    }

    [Fact]
    public void TotalHistoricalBlocks_ShouldReturnZero_WhenNoBlocksExist()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var history = new PenaltyHistory([
            PatientPenalty.CreateAutomaticWarning(patientId, Guid.NewGuid(), "No show"),
        ]);

        // Assert
        history.TotalHistoricalBlocks.Should().Be(0);
    }

    [Fact]
    public void TotalHistoricalBlocks_ShouldCountAllBlocks_RegardlessOfState()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        var expiredBlock = PatientPenalty.CreateAutomaticBlock(
            patientId,
            PenaltyReasons.AutomaticBlock,
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(-1).Date,
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(-6).Date
        );

        var activeBlock = PatientPenalty.CreateAutomaticBlock(
            patientId,
            PenaltyReasons.AutomaticBlock,
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(10).Date,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var history = new PenaltyHistory([expiredBlock, activeBlock]);

        // Assert
        history.TotalHistoricalBlocks.Should().Be(2);
    }

    [Fact]
    public void IsCurrentlyBlocked_ShouldReturnFalse_WhenNoActiveBlocks()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        var expiredBlock = PatientPenalty.CreateAutomaticBlock(
            patientId,
            PenaltyReasons.AutomaticBlock,
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(-1).Date,
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(-6).Date
        );

        var history = new PenaltyHistory([expiredBlock]);

        // Assert
        history.IsCurrentlyBlocked(_fakeTime.GetUtcNow().UtcDateTime).Should().BeFalse();
    }

    [Fact]
    public void IsCurrentlyBlocked_ShouldReturnTrue_WhenActiveBlockExists()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        var activeBlock = PatientPenalty.CreateAutomaticBlock(
            patientId,
            PenaltyReasons.AutomaticBlock,
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(10).Date,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var history = new PenaltyHistory([activeBlock]);

        // Assert
        history.IsCurrentlyBlocked(_fakeTime.GetUtcNow().UtcDateTime).Should().BeTrue();
    }

    [Fact]
    public void IsCurrentlyBlocked_ShouldReturnFalse_WhenBlockIsRemoved()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        var removedBlock = PatientPenalty.CreateAutomaticBlock(
            patientId,
            PenaltyReasons.AutomaticBlock,
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(10).Date,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        removedBlock.Remove();

        var history = new PenaltyHistory([removedBlock]);

        // Assert
        history.IsCurrentlyBlocked(_fakeTime.GetUtcNow().UtcDateTime).Should().BeFalse();
    }

    [Fact]
    public void DetermineNextBlockDuration_ShouldReturnMinor_WhenNoHistoricalBlocks()
    {
        // Arrange
        var history = new PenaltyHistory([]);

        // Assert
        history.DetermineNextBlockDuration().Should().Be(BlockDuration.Minor);
    }

    [Fact]
    public void DetermineNextBlockDuration_ShouldReturnModerate_WhenOneHistoricalBlock()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        var history = new PenaltyHistory([
            PatientPenalty.CreateAutomaticBlock(
                patientId,
                PenaltyReasons.AutomaticBlock,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-1).Date,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-6).Date
            ),
        ]);

        // Assert
        history.DetermineNextBlockDuration().Should().Be(BlockDuration.Moderate);
    }

    [Fact]
    public void DetermineNextBlockDuration_ShouldReturnSevere_WhenTwoOrMoreHistoricalBlocks()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        var history = new PenaltyHistory([
            PatientPenalty.CreateAutomaticBlock(
                patientId,
                PenaltyReasons.AutomaticBlock,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-20).Date,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-25).Date
            ),
            PatientPenalty.CreateAutomaticBlock(
                patientId,
                PenaltyReasons.AutomaticBlock,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-1).Date,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-16).Date
            ),
        ]);

        // Assert
        history.DetermineNextBlockDuration().Should().Be(BlockDuration.Severe);
    }

    [Fact]
    public void DetermineNextBlockDuration_ShouldCapAtSevere_WhenManyHistoricalBlocks()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        var history = new PenaltyHistory([
            PatientPenalty.CreateAutomaticBlock(
                patientId,
                PenaltyReasons.AutomaticBlock,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-50).Date,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-55).Date
            ),
            PatientPenalty.CreateAutomaticBlock(
                patientId,
                PenaltyReasons.AutomaticBlock,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-30).Date,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-45).Date
            ),
            PatientPenalty.CreateAutomaticBlock(
                patientId,
                PenaltyReasons.AutomaticBlock,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-1).Date,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-31).Date
            ),
        ]);

        // Assert
        history.DetermineNextBlockDuration().Should().Be(BlockDuration.Severe);
    }
}
