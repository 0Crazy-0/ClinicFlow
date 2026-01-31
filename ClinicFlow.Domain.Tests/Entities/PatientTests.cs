using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.Entities;

public class PatientTests
{
    #region EnsureNotBlocked
    [Fact]
    public void EnsureNotBlocked_ShouldNotThrow_WhenNoPenalties()
    {
        // Arrange
        var patient = CreatePatient();
        var penalties = new List<PatientPenalty>();

        // Act
        var act = () => patient.EnsureNotBlocked(penalties);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureNotBlocked_ShouldNotThrow_WhenOnlyWarnings()
    {
        // Arrange
        var patient = CreatePatient();
        var penalties = new List<PatientPenalty>
        {
            PatientPenalty.CreateWarning(patient.Id, Guid.NewGuid(), "Warning 1"),
            PatientPenalty.CreateWarning(patient.Id, Guid.NewGuid(), "Warning 2")
        };

        // Act
        var act = () => patient.EnsureNotBlocked(penalties);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureNotBlocked_ShouldNotThrow_WhenBlockExpired()
    {
        // Arrange
        var patient = CreatePatient();
        var penalties = new List<PatientPenalty>
        {
            PatientPenalty.CreateBlock(patient.Id, "Old Block", DateTime.UtcNow.AddDays(-1))
        };

        // Act
        var act = () => patient.EnsureNotBlocked(penalties);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureNotBlocked_ShouldThrowPatientBlockedException_WhenActiveBlockExists()
    {
        // Arrange
        var patient = CreatePatient();
        var blockedUntil = DateTime.UtcNow.AddDays(10);
        var penalties = new List<PatientPenalty>
        {
            PatientPenalty.CreateBlock(patient.Id, "Active Block", blockedUntil)
        };

        // Act
        var act = () => patient.EnsureNotBlocked(penalties);

        // Assert
        act.Should().Throw<PatientBlockedException>().Where(e => e.BlockedUntil == blockedUntil);
    }
    #endregion

    #region Helpers
    private Patient CreatePatient() => new(Guid.NewGuid(), DateTime.UtcNow.AddYears(-30), "O+", "None", "None", "Mom", "555-5555");
    #endregion
}
