using ClinicFlow.Application.Penalties.Queries.GetPenaltiesByPatientId;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Penalties.Queries.GetPenaltiesByPatientId;

public class GetPenaltiesByPatientIdQueryHandlerTests
{
    private readonly Mock<IPatientPenaltyRepository> _penaltyRepositoryMock;
    private readonly GetPenaltiesByPatientIdQueryHandler _sut;

    public GetPenaltiesByPatientIdQueryHandlerTests()
    {
        _penaltyRepositoryMock = new Mock<IPatientPenaltyRepository>();
        _sut = new GetPenaltiesByPatientIdQueryHandler(_penaltyRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPenalties_WhenPenaltiesExist()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var warning = PatientPenalty.CreateAutomaticWarning(patientId, Guid.NewGuid(), "Warning 1");
        var block = PatientPenalty.CreateManualBlock(
            patientId,
            "Block 1",
            BlockDuration.Minor,
            DateTime.UtcNow
        );

        _penaltyRepositoryMock
            .Setup(x => x.GetByPatientIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([warning, block]);

        var query = new GetPenaltiesByPatientIdQuery(patientId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var resultList = result.ToList();
        resultList[0].Id.Should().Be(warning.Id);
        resultList[0].PatientId.Should().Be(patientId);
        resultList[0].Type.Should().Be(nameof(PenaltyType.Warning));

        resultList[1].Id.Should().Be(block.Id);
        resultList[1].Type.Should().Be(nameof(PenaltyType.TemporaryBlock));
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoPenaltiesExist()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        _penaltyRepositoryMock
            .Setup(x => x.GetByPatientIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var query = new GetPenaltiesByPatientIdQuery(patientId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
