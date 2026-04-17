using ClinicFlow.Application.Penalties.Commands.RemovePenalty;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Penalties.Commands.RemovePenalty;

public class RemovePenaltyCommandHandlerTests
{
    private readonly Mock<IPatientPenaltyRepository> _penaltyRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FakeTimeProvider _fakeTime;
    private readonly RemovePenaltyCommandHandler _sut;

    public RemovePenaltyCommandHandlerTests()
    {
        _penaltyRepositoryMock = new Mock<IPatientPenaltyRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fakeTime = new FakeTimeProvider();
        _sut = new RemovePenaltyCommandHandler(
            _penaltyRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldRemovePenalty_WhenPenaltyExists()
    {
        // Arrange
        var penalty = PatientPenalty.CreateManualBlock(
            Guid.NewGuid(),
            "Block reason",
            BlockDuration.Minor,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _penaltyRepositoryMock
            .Setup(x => x.GetByIdAsync(penalty.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(penalty);

        var command = new RemovePenaltyCommand(penalty.Id);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        penalty.IsRemoved.Should().BeTrue();

        _penaltyRepositoryMock.Verify(
            x => x.UpdateAsync(penalty, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenPenaltyDoesNotExist()
    {
        // Arrange
        var penaltyId = Guid.NewGuid();
        _penaltyRepositoryMock
            .Setup(x => x.GetByIdAsync(penaltyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PatientPenalty?)null);

        var command = new RemovePenaltyCommand(penaltyId);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(PatientPenalty));

        _penaltyRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<PatientPenalty>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
