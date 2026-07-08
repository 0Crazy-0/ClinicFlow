using AwesomeAssertions;
using ClinicFlow.Application.Penalties.Queries.GetActiveBlockedPatients;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
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
    public async Task Handle_ShouldReturnPaginatedList_WhenActiveBlocksExist()
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
            .Setup(x =>
                x.GetActiveBlocksPaginatedAsync(
                    It.IsAny<DateOnly>(),
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(([block1, block2], 2));

        var query = new GetActiveBlockedPatientsQuery(1, 10);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.Items.Should().HaveCount(2);

        var resultList = result.Items.ToList();
        resultList[0].Id.Should().Be(block1.Id);
        resultList[0].Type.Should().Be(nameof(PenaltyType.TemporaryBlock));

        resultList[1].Id.Should().Be(block2.Id);
        resultList[1].Type.Should().Be(nameof(PenaltyType.TemporaryBlock));

        _penaltyRepositoryMock.Verify(
            x =>
                x.GetActiveBlocksPaginatedAsync(
                    It.IsAny<DateOnly>(),
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPaginatedList_WhenNoActiveBlocksExist()
    {
        // Arrange
        _penaltyRepositoryMock
            .Setup(x =>
                x.GetActiveBlocksPaginatedAsync(
                    It.IsAny<DateOnly>(),
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<PatientPenalty>(), 0));

        var query = new GetActiveBlockedPatientsQuery(1, 10);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);

        _penaltyRepositoryMock.Verify(
            x =>
                x.GetActiveBlocksPaginatedAsync(
                    It.IsAny<DateOnly>(),
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
