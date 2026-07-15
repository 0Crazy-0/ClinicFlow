using AwesomeAssertions;
using ClinicFlow.Application.Penalties.Queries.DTOs;
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
        var patientId = Guid.CreateVersion7();
        var warning = PatientPenalty.CreateAutomaticWarning(
            patientId,
            Guid.CreateVersion7(),
            "Warning 1"
        );
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
        var expectedDtos = new List<PatientPenalty> { warning, block }.Select(
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
            x =>
                x.GetHistoryByPatientIdPaginatedAsync(
                    patientId,
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPaginatedList_WhenNoPenaltiesExist()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();

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
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(0);

        _penaltyRepositoryMock.Verify(
            x =>
                x.GetHistoryByPatientIdPaginatedAsync(
                    patientId,
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
