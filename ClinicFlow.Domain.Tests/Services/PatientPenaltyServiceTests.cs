using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Services;
using Moq;

namespace ClinicFlow.Domain.Tests.Services;

public class PatientPenaltyServiceTests
{
    private readonly Mock<IPatientPenaltyRepository> _repositoryMock;
    private readonly PatientPenaltyService _service;

    public PatientPenaltyServiceTests()
    {
        _repositoryMock = new Mock<IPatientPenaltyRepository>();
        _service = new PatientPenaltyService(_repositoryMock.Object);
    }
    
    [Fact]
    public async Task ApplyPenaltyAsync_ShouldAddWarning_WhenCalled()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var reason = "No show";

        _repositoryMock.Setup(x => x.GetByPatientIdAsync(patientId)).ReturnsAsync([]);

        // Act
        await _service.ApplyPenaltyAsync(patientId, appointmentId, reason);

        // Assert
        _repositoryMock.Verify(x => x.AddAsync(It.Is<PatientPenalty>(p => p.PatientId == patientId && p.PenaltyType == PenaltyTypeEnum.Warning && p.Reason == reason)),
            Times.Once);
    }

    [Fact]
    public async Task ApplyPenaltyAsync_ShouldAddBlock_WhenStrikesThresholdReached()
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

        _repositoryMock.Setup(x => x.GetByPatientIdAsync(patientId)).ReturnsAsync(existingPenalties);

        // Act
        // This call will add the 3rd warning, triggering the block logic
        await _service.ApplyPenaltyAsync(patientId, appointmentId, "Warning 3");

        // Assert
        // Verify warning was added
        _repositoryMock.Verify(x => x.AddAsync(It.Is<PatientPenalty>(p => p.PenaltyType == PenaltyTypeEnum.Warning)), Times.Once);

        // Verify block was added
        _repositoryMock.Verify(x => x.AddAsync(It.Is<PatientPenalty>(p => p.PenaltyType == PenaltyTypeEnum.TemporaryBlock && p.Reason == "Automatic block due to 3 strikes")),
             Times.Once);
    }

    [Fact]
    public async Task ApplyPenaltyAsync_ShouldNotAddBlock_WhenAlreadyBlocked()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var existingPenalties = new List<PatientPenalty>
        {
            PatientPenalty.CreateWarning(patientId, Guid.NewGuid(), "Warning 1"),
            PatientPenalty.CreateWarning(patientId, Guid.NewGuid(), "Warning 2"),
            PatientPenalty.CreateBlock(patientId, "Existing Block", DateTime.UtcNow.AddDays(10))
        };

        _repositoryMock.Setup(x => x.GetByPatientIdAsync(patientId)).ReturnsAsync(existingPenalties);

        // Act
        await _service.ApplyPenaltyAsync(patientId, Guid.NewGuid(), "Warning 3");

        // Assert
        // Verify warning was added
        _repositoryMock.Verify(x => x.AddAsync(It.Is<PatientPenalty>(p => p.PenaltyType == PenaltyTypeEnum.Warning)), Times.Once);

        // Verify NO NEW block was added
        _repositoryMock.Verify(x => x.AddAsync(It.Is<PatientPenalty>(p => p.PenaltyType == PenaltyTypeEnum.TemporaryBlock)), Times.Never);
    }
}
