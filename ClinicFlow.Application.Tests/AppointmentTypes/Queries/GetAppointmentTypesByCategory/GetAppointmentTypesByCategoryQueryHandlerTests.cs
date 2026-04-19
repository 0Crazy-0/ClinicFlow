using ClinicFlow.Application.AppointmentTypes.Queries.GetAppointmentTypesByCategory;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Queries.GetAppointmentTypesByCategory;

public class GetAppointmentTypesByCategoryQueryHandlerTests
{
    private readonly Mock<IAppointmentTypeDefinitionRepository> _repositoryMock;
    private readonly GetAppointmentTypesByCategoryQueryHandler _sut;

    public GetAppointmentTypesByCategoryQueryHandlerTests()
    {
        _repositoryMock = new Mock<IAppointmentTypeDefinitionRepository>();
        _sut = new GetAppointmentTypesByCategoryQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMatchingTypes_WhenCategoryHasTypes()
    {
        // Arrange
        var type1 = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "General Checkup",
            "Routine",
            TimeSpan.FromMinutes(30)
        );

        _repositoryMock
            .Setup(x =>
                x.GetByCategoryAsync(AppointmentCategory.Checkup, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync([type1]);

        var query = new GetAppointmentTypesByCategoryQuery(AppointmentCategory.Checkup);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().ContainSingle();
        result[0].Category.Should().Be(nameof(AppointmentCategory.Checkup));
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenCategoryHasNoTypes()
    {
        // Arrange
        _repositoryMock
            .Setup(x =>
                x.GetByCategoryAsync(AppointmentCategory.Emergency, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync([]);

        var query = new GetAppointmentTypesByCategoryQuery(AppointmentCategory.Emergency);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
