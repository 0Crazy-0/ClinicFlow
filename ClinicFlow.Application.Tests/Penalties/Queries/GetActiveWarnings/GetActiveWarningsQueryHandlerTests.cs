using ClinicFlow.Application.Penalties.Queries.GetActiveWarnings;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
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
    public async Task Handle_ShouldReturnWarnings_WhenActiveWarningsExist()
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
            .Setup(x => x.GetActiveWarningsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([warning1, warning2]);

        var query = new GetActiveWarningsQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var resultList = result.ToList();
        resultList[0].Id.Should().Be(warning1.Id);
        resultList[0].Type.Should().Be(nameof(PenaltyType.Warning));

        resultList[1].Id.Should().Be(warning2.Id);
        resultList[1].Type.Should().Be(nameof(PenaltyType.Warning));
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoActiveWarningsExist()
    {
        // Arrange
        _penaltyRepositoryMock
            .Setup(x => x.GetActiveWarningsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var query = new GetActiveWarningsQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
