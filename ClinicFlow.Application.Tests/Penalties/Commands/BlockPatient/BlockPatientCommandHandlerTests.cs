using ClinicFlow.Application.Penalties.Commands.BlockPatient;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Penalties.Commands.BlockPatient;

public class BlockPatientCommandHandlerTests
{
    private readonly Mock<IPatientPenaltyRepository> _penaltyRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FakeTimeProvider _fakeTime;
    private readonly BlockPatientCommandHandler _sut;

    public BlockPatientCommandHandlerTests()
    {
        _penaltyRepositoryMock = new Mock<IPatientPenaltyRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fakeTime = new FakeTimeProvider();
        _sut = new BlockPatientCommandHandler(
            _fakeTime,
            _penaltyRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Theory]
    [InlineData(BlockDuration.Minor, 5)]
    [InlineData(BlockDuration.Moderate, 15)]
    [InlineData(BlockDuration.Severe, 30)]
    public async Task Handle_ShouldCreateManualBlock_WithCorrectDuration(
        BlockDuration duration,
        int expectedDays
    )
    {
        // Arrange
        var command = new BlockPatientCommand(
            Guid.NewGuid(),
            "Patient was rude to staff",
            duration
        );

        PatientPenalty? capturedPenalty = null;
        _penaltyRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<PatientPenalty>(), It.IsAny<CancellationToken>()))
            .Callback<PatientPenalty, CancellationToken>((p, _) => capturedPenalty = p);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        capturedPenalty.Should().NotBeNull();
        capturedPenalty!.PatientId.Should().Be(command.PatientId);
        capturedPenalty.Reason.Should().Be(command.Reason);
        capturedPenalty.Type.Should().Be(PenaltyType.TemporaryBlock);
        capturedPenalty
            .BlockedUntil.Should()
            .Be(_fakeTime.GetUtcNow().UtcDateTime.Date.AddDays(expectedDays));
        capturedPenalty.IsRemoved.Should().BeFalse();

        _penaltyRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<PatientPenalty>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
