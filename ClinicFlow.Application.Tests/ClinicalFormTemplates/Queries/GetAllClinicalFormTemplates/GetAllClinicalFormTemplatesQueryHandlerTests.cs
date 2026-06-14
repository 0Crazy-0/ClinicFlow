using AwesomeAssertions;
using ClinicFlow.Application.ClinicalFormTemplates.Queries.GetAllClinicalFormTemplates;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using Moq;

namespace ClinicFlow.Application.Tests.ClinicalFormTemplates.Queries.GetAllClinicalFormTemplates;

public class GetAllClinicalFormTemplatesQueryHandlerTests
{
    private readonly Mock<IClinicalFormTemplateRepository> _repositoryMock;
    private readonly GetAllClinicalFormTemplatesQueryHandler _sut;

    public GetAllClinicalFormTemplatesQueryHandlerTests()
    {
        _repositoryMock = new Mock<IClinicalFormTemplateRepository>();
        _sut = new GetAllClinicalFormTemplatesQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllTemplatesIncludingInactive_WhenTemplatesExist()
    {
        // Arrange
        var activeTemplate = ClinicalFormTemplate.Create(
            "INTAKE_V1",
            "Intake Form",
            "Initial intake",
            "{}"
        );
        var inactiveTemplate = ClinicalFormTemplate.Create(
            "DEPRECATED_V1",
            "Deprecated Form",
            "Old form",
            "{}"
        );
        inactiveTemplate.Deactivate();

        _repositoryMock
            .Setup(x => x.GetAllIncludingDeletedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([activeTemplate, inactiveTemplate]);

        var query = new GetAllClinicalFormTemplatesQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].IsDeleted.Should().BeFalse();
        result[1].IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoTemplatesExist()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetAllIncludingDeletedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _sut.Handle(
            new GetAllClinicalFormTemplatesQuery(),
            CancellationToken.None
        );

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
