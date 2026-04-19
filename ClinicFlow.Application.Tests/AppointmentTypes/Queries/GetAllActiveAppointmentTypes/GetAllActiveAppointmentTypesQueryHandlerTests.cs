using ClinicFlow.Application.AppointmentTypes.Queries.GetAllActiveAppointmentTypes;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Queries.GetAllActiveAppointmentTypes;

public class GetAllActiveAppointmentTypesQueryHandlerTests
{
    private readonly Mock<IAppointmentTypeDefinitionRepository> _repositoryMock;
    private readonly GetAllActiveAppointmentTypesQueryHandler _sut;

    public GetAllActiveAppointmentTypesQueryHandlerTests()
    {
        _repositoryMock = new Mock<IAppointmentTypeDefinitionRepository>();
        _sut = new GetAllActiveAppointmentTypesQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllActiveTypes_WhenTypesExist()
    {
        // Arrange
        var type1 = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "General Checkup",
            "Routine",
            TimeSpan.FromMinutes(30)
        );
        var type2 = AppointmentTypeDefinition.Create(
            AppointmentCategory.FollowUp,
            "Follow Up",
            "Return visit",
            TimeSpan.FromMinutes(20)
        );

        _repositoryMock
            .Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([type1, type2]);

        var query = new GetAllActiveAppointmentTypesQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("General Checkup");
        result[1].Name.Should().Be("Follow Up");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoTypesExist()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _sut.Handle(
            new GetAllActiveAppointmentTypesQuery(),
            CancellationToken.None
        );

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
