using AwesomeAssertions;
using ClinicFlow.Application.ClinicalFormTemplates.Queries.GetAllActiveClinicalFormTemplates;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using Moq;

namespace ClinicFlow.Application.Tests.ClinicalFormTemplates.Queries.GetAllActiveClinicalFormTemplates;

public class GetAllActiveClinicalFormTemplatesQueryHandlerTests
{
    private readonly Mock<IClinicalFormTemplateRepository> _repositoryMock;
    private readonly GetAllActiveClinicalFormTemplatesQueryHandler _sut;

    public GetAllActiveClinicalFormTemplatesQueryHandlerTests()
    {
        _repositoryMock = new Mock<IClinicalFormTemplateRepository>();
        _sut = new GetAllActiveClinicalFormTemplatesQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllActiveTemplates_WhenTemplatesExist()
    {
        // Arrange
        var template1 = ClinicalFormTemplate.Create(
            "INTAKE_V1",
            "Intake Form",
            "Initial intake",
            "{}"
        );
        var template2 = ClinicalFormTemplate.Create(
            "CARDIO_V1",
            "Cardiology Form",
            "Cardiology assessment",
            "{}"
        );

        _repositoryMock
            .Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([template1, template2]);

        var query = new GetAllActiveClinicalFormTemplatesQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be(template1.Name);
        result[1].Name.Should().Be(template2.Name);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoTemplatesExist()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _sut.Handle(
            new GetAllActiveClinicalFormTemplatesQuery(),
            CancellationToken.None
        );

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
