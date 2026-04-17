using ClinicFlow.Application.Penalties.Queries.GetActiveBlockedPatients;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Penalties.Queries.GetActiveBlockedPatients;

public class GetActiveBlockedPatientsQueryHandlerTests
{
    private readonly Mock<IPatientPenaltyRepository> _penaltyRepositoryMock;
    private readonly FakeTimeProvider _fakeTime;
    private readonly GetActiveBlockedPatientsQueryHandler _sut;

    public GetActiveBlockedPatientsQueryHandlerTests()
    {
        _penaltyRepositoryMock = new Mock<IPatientPenaltyRepository>();
        _fakeTime = new FakeTimeProvider();
        _sut = new GetActiveBlockedPatientsQueryHandler(_fakeTime, _penaltyRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnBlockedPatients_WhenActiveBlocksExist()
    {
        // Arrange
        var block1 = PatientPenalty.CreateManualBlock(
            Guid.NewGuid(),
            "Rude behavior",
            BlockDuration.Minor,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var block2 = PatientPenalty.CreateManualBlock(
            Guid.NewGuid(),
            "Repeated no-shows",
            BlockDuration.Severe,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _penaltyRepositoryMock
            .Setup(x => x.GetActiveBlocksAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([block1, block2]);

        var query = new GetActiveBlockedPatientsQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var resultList = result.ToList();
        resultList[0].Id.Should().Be(block1.Id);
        resultList[0].Type.Should().Be(nameof(PenaltyType.TemporaryBlock));

        resultList[1].Id.Should().Be(block2.Id);
        resultList[1].Type.Should().Be(nameof(PenaltyType.TemporaryBlock));
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoActiveBlocksExist()
    {
        // Arrange
        _penaltyRepositoryMock
            .Setup(x => x.GetActiveBlocksAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var query = new GetActiveBlockedPatientsQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
