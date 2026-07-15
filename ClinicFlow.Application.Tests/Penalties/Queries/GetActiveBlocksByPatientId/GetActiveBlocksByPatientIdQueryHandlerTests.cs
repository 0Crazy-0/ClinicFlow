using AwesomeAssertions;
using ClinicFlow.Application.Penalties.Queries.DTOs;
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
        var patientId = Guid.CreateVersion7();
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
        var expectedDtos = new List<PatientPenalty> { block }.Select(p => new PatientPenaltyDto(
            p.Id,
            p.PatientId,
            p.AppointmentId,
            p.Type.ToString(),
            p.Reason,
            p.BlockedUntil,
            p.IsRemoved
        ));

        result.Should().BeEquivalentTo(expectedDtos);

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
        var patientId = Guid.CreateVersion7();
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
