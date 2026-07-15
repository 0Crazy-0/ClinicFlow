using AwesomeAssertions;
using ClinicFlow.Application.Penalties.Queries.DTOs;
using ClinicFlow.Application.Penalties.Queries.GetActiveWarnings;
using ClinicFlow.Domain.Entities;
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
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "No show"
        );
        var warning2 = PatientPenalty.CreateAutomaticWarning(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Late cancellation"
        );

        _penaltyRepositoryMock
            .Setup(x => x.GetActiveWarningsPaginatedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(([warning1, warning2], 2));

        var query = new GetActiveWarningsQuery(1, 10);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDtos = new List<PatientPenalty> { warning1, warning2 }.Select(
            p => new PatientPenaltyDto(
                p.Id,
                p.PatientId,
                p.AppointmentId,
                p.Type.ToString(),
                p.Reason,
                p.BlockedUntil,
                p.IsRemoved
            )
        );

        result.Items.Should().BeEquivalentTo(expectedDtos);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(1);

        _penaltyRepositoryMock.Verify(
            x => x.GetActiveWarningsPaginatedAsync(1, 10, It.IsAny<CancellationToken>()),
            Times.Once
        );
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
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(0);

        _penaltyRepositoryMock.Verify(
            x => x.GetActiveWarningsPaginatedAsync(1, 10, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
