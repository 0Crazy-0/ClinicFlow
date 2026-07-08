using AwesomeAssertions;
using ClinicFlow.Application.Penalties.Queries.GetActiveBlocksByPatientId;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Penalties.Queries.GetActiveBlocksByPatientId;

public class GetActiveBlocksByPatientIdQueryHandlerTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IPatientPenaltyRepository> _penaltyRepositoryMock;
    private readonly GetActiveBlocksByPatientIdQueryHandler _sut;

    public GetActiveBlocksByPatientIdQueryHandlerTests()
    {
        _penaltyRepositoryMock = new Mock<IPatientPenaltyRepository>();
        _sut = new GetActiveBlocksByPatientIdQueryHandler(_fakeTime, _penaltyRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnActiveBlocks_WhenActiveBlocksExist()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var block = PatientPenalty.CreateManualBlock(
            patientId,
            "Block 1",
            BlockDuration.Minor,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _penaltyRepositoryMock
            .Setup(x =>
                x.GetActiveBlocksByPatientIdAsync(
                    patientId,
                    referenceDate,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([block]);

        var query = new GetActiveBlocksByPatientIdQuery(patientId);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainSingle();

        var resultList = result.ToList();
        resultList[0].Id.Should().Be(block.Id);
        resultList[0].PatientId.Should().Be(patientId);
        resultList[0].Type.Should().Be(nameof(PenaltyType.TemporaryBlock));

        _penaltyRepositoryMock.Verify(
            x =>
                x.GetActiveBlocksByPatientIdAsync(
                    patientId,
                    referenceDate,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoActiveBlocksExist()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        _penaltyRepositoryMock
            .Setup(x =>
                x.GetActiveBlocksByPatientIdAsync(
                    patientId,
                    referenceDate,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([]);

        var query = new GetActiveBlocksByPatientIdQuery(patientId);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _penaltyRepositoryMock.Verify(
            x =>
                x.GetActiveBlocksByPatientIdAsync(
                    patientId,
                    referenceDate,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
