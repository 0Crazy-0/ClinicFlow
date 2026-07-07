using AwesomeAssertions;
using ClinicFlow.Application.Penalties.Queries.GetPenaltyHistoryByPatientId;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Penalties.Queries.GetPenaltyHistoryByPatientId;

public class GetPenaltyHistoryByPatientIdQueryHandlerTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IPatientPenaltyRepository> _penaltyRepositoryMock;
    private readonly GetPenaltyHistoryByPatientIdQueryHandler _sut;

    public GetPenaltyHistoryByPatientIdQueryHandlerTests()
    {
        _penaltyRepositoryMock = new Mock<IPatientPenaltyRepository>();
        _sut = new GetPenaltyHistoryByPatientIdQueryHandler(_penaltyRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedList_WhenPenaltiesExist()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var warning = PatientPenalty.CreateAutomaticWarning(patientId, Guid.NewGuid(), "Warning 1");
        var block = PatientPenalty.CreateManualBlock(
            patientId,
            "Block 1",
            BlockDuration.Minor,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _penaltyRepositoryMock
            .Setup(x =>
                x.GetHistoryByPatientIdPaginatedAsync(
                    patientId,
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(([warning, block], 2));

        var query = new GetPenaltyHistoryByPatientIdQuery(patientId, 1, 10);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.Items.Should().HaveCount(2);

        var resultList = result.Items.ToList();
        resultList[0].Id.Should().Be(warning.Id);
        resultList[0].PatientId.Should().Be(patientId);
        resultList[0].Type.Should().Be(nameof(PenaltyType.Warning));

        resultList[1].Id.Should().Be(block.Id);
        resultList[1].Type.Should().Be(nameof(PenaltyType.TemporaryBlock));
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPaginatedList_WhenNoPenaltiesExist()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        _penaltyRepositoryMock
            .Setup(x =>
                x.GetHistoryByPatientIdPaginatedAsync(
                    patientId,
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<PatientPenalty>(), 0));

        var query = new GetPenaltyHistoryByPatientIdQuery(patientId, 1, 10);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }
}
