using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.ValueObjects;
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
            BlockDuration.Minor,
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(-6)
        );

        var activeBlock = PatientPenalty.CreateAutomaticBlock(
            patientId,
            PenaltyReasons.AutomaticBlock,
            BlockDuration.Minor,
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
            BlockDuration.Minor,
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(-6)
        );

        var history = new PenaltyHistory([expiredBlock]);

        // Assert
        history
            .IsCurrentlyBlocked(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime))
            .Should()
            .BeFalse();
    }

    [Fact]
    public void IsCurrentlyBlocked_ShouldReturnTrue_WhenActiveBlockExists()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        var activeBlock = PatientPenalty.CreateAutomaticBlock(
            patientId,
            PenaltyReasons.AutomaticBlock,
            BlockDuration.Minor,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var history = new PenaltyHistory([activeBlock]);

        // Assert
        history
            .IsCurrentlyBlocked(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime))
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsCurrentlyBlocked_ShouldReturnFalse_WhenBlockIsRemoved()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        var removedBlock = PatientPenalty.CreateAutomaticBlock(
            patientId,
            PenaltyReasons.AutomaticBlock,
            BlockDuration.Minor,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        removedBlock.Remove();

        var history = new PenaltyHistory([removedBlock]);

        // Assert
        history
            .IsCurrentlyBlocked(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime))
            .Should()
            .BeFalse();
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
                BlockDuration.Minor,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-6)
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
                BlockDuration.Minor,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-25)
            ),
            PatientPenalty.CreateAutomaticBlock(
                patientId,
                PenaltyReasons.AutomaticBlock,
                BlockDuration.Minor,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-6)
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
                BlockDuration.Minor,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-55)
            ),
            PatientPenalty.CreateAutomaticBlock(
                patientId,
                PenaltyReasons.AutomaticBlock,
                BlockDuration.Minor,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-35)
            ),
            PatientPenalty.CreateAutomaticBlock(
                patientId,
                PenaltyReasons.AutomaticBlock,
                BlockDuration.Minor,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-6)
            ),
        ]);

        // Assert
        history.DetermineNextBlockDuration().Should().Be(BlockDuration.Severe);
    }

    [Fact]
    public void EnsureNotBlocked_ShouldNotThrow_WhenNoPenalties()
    {
        // Arrange
        var history = new PenaltyHistory([]);

        // Act
        var act = () =>
            history.EnsureNotBlocked(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime));

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureNotBlocked_ShouldNotThrow_WhenOnlyWarnings()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var penalties = new List<PatientPenalty>
        {
            PatientPenalty.CreateAutomaticWarning(patientId, Guid.NewGuid(), "Warning 1"),
            PatientPenalty.CreateAutomaticWarning(patientId, Guid.NewGuid(), "Warning 2"),
        };

        var history = new PenaltyHistory(penalties);

        // Act
        var act = () =>
            history.EnsureNotBlocked(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime));

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureNotBlocked_ShouldNotThrow_WhenBlockExpired()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var penalties = new List<PatientPenalty>
        {
            PatientPenalty.CreateAutomaticBlock(
                patientId,
                PenaltyReasons.AutomaticBlock,
                BlockDuration.Minor,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-6)
            ),
        };
        var history = new PenaltyHistory(penalties);

        // Act
        var act = () =>
            history.EnsureNotBlocked(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime));

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureNotBlocked_ShouldThrowPatientBlockedException_WhenActiveBlockExists()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var blockedUntil = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime).AddDays(5);
        var penalties = new List<PatientPenalty>
        {
            PatientPenalty.CreateAutomaticBlock(
                patientId,
                PenaltyReasons.AutomaticBlock,
                BlockDuration.Minor,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
        };

        var history = new PenaltyHistory(penalties);

        // Act
        var act = () =>
            history.EnsureNotBlocked(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime));

        // Assert
        act.Should()
            .Throw<PatientBlockedException>()
            .WithMessage(DomainErrors.Patient.Blocked)
            .Where(e => e.BlockedUntil == blockedUntil);
    }

    [Fact]
    public void EnsureNotBlocked_ShouldNotThrow_WhenActiveBlockIsRemoved()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var penalty = PatientPenalty.CreateAutomaticBlock(
            patientId,
            PenaltyReasons.AutomaticBlock,
            BlockDuration.Minor,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        penalty.Remove();

        var penalties = new List<PatientPenalty> { penalty };
        var history = new PenaltyHistory(penalties);

        // Act
        var act = () =>
            history.EnsureNotBlocked(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime));

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureNotBlocked_ShouldNotThrow_WhenBlockExpiresExactlyOnReferenceDate()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var penalty = PatientPenalty.CreateAutomaticBlock(
            patientId,
            PenaltyReasons.AutomaticBlock,
            BlockDuration.Minor,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var referenceDate = penalty.BlockedUntil!.Value;
        var penalties = new List<PatientPenalty> { penalty };
        var history = new PenaltyHistory(penalties);

        // Act
        var act = () => history.EnsureNotBlocked(referenceDate);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureNotBlocked_ShouldThrowPatientBlockedExceptionWithFurthestDate_WhenMultipleActiveBlocksExistWithDifferentExpirationDates()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var penaltyMinor = PatientPenalty.CreateAutomaticBlock(
            patientId,
            PenaltyReasons.AutomaticBlock,
            BlockDuration.Minor,
            referenceTime
        );

        var penaltySevere = PatientPenalty.CreateAutomaticBlock(
            patientId,
            PenaltyReasons.AutomaticBlock,
            BlockDuration.Severe,
            referenceTime
        );

        var penalties = new List<PatientPenalty> { penaltyMinor, penaltySevere };
        var referenceDate = DateOnly.FromDateTime(referenceTime);
        var expectedFurthestBlockedUntil = penaltySevere.BlockedUntil!.Value;
        var history = new PenaltyHistory(penalties);

        // Act
        var act = () => history.EnsureNotBlocked(referenceDate);

        // Assert
        act.Should()
            .Throw<PatientBlockedException>()
            .WithMessage(DomainErrors.Patient.Blocked)
            .Where(e => e.BlockedUntil == expectedFurthestBlockedUntil);
    }
}
