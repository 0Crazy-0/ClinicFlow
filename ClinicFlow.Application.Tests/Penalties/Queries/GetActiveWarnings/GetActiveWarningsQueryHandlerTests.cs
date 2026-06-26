using AwesomeAssertions;
using ClinicFlow.Application.Penalties.Queries.GetActiveWarnings;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using Moq;

namespace ClinicFlow.Application.Tests.Penalties.Queries.GetActiveWarnings;

public class GetActiveWarningsQueryHandlerTests
{
    private readonly Mock<IPatientPenaltyRepository> _penaltyRepositoryMock;
    private readonly GetActiveWarningsQueryHandler _sut;

    public GetActiveWarningsQueryHandlerTests()
    {
        _penaltyRepositoryMock = new Mock<IPatientPenaltyRepository>();
        _sut = new GetActiveWarningsQueryHandler(_penaltyRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedList_WhenActiveWarningsExist()
    {
        // Arrange
        var warning1 = PatientPenalty.CreateAutomaticWarning(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "No show"
        );
        var warning2 = PatientPenalty.CreateAutomaticWarning(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Late cancellation"
        );

        _penaltyRepositoryMock
            .Setup(x => x.GetActiveWarningsPaginatedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(([warning1, warning2], 2));

        var query = new GetActiveWarningsQuery(1, 10);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.Items.Should().HaveCount(2);

        var resultList = result.Items.ToList();
        resultList[0].Id.Should().Be(warning1.Id);
        resultList[0].Type.Should().Be(nameof(PenaltyType.Warning));

        resultList[1].Id.Should().Be(warning2.Id);
        resultList[1].Type.Should().Be(nameof(PenaltyType.Warning));
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPaginatedList_WhenNoActiveWarningsExist()
    {
        // Arrange
        _penaltyRepositoryMock
            .Setup(x => x.GetActiveWarningsPaginatedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<PatientPenalty>(), 0));

        var query = new GetActiveWarningsQuery(1, 10);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }
}
