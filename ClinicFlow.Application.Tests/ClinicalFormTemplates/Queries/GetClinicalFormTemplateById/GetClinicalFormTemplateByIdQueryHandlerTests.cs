using AwesomeAssertions;
using ClinicFlow.Application.ClinicalFormTemplates.Queries.DTOs;
using ClinicFlow.Application.ClinicalFormTemplates.Queries.GetClinicalFormTemplateById;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using Moq;

namespace ClinicFlow.Application.Tests.ClinicalFormTemplates.Queries.GetClinicalFormTemplateById;

public class GetClinicalFormTemplateByIdQueryHandlerTests
{
    private readonly Mock<IClinicalFormTemplateRepository> _repositoryMock;
    private readonly GetClinicalFormTemplateByIdQueryHandler _sut;

    public GetClinicalFormTemplateByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IClinicalFormTemplateRepository>();
        _sut = new GetClinicalFormTemplateByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDto_WhenTemplateExists()
    {
        // Arrange
        var template = ClinicalFormTemplate.Create(
            "INTAKE_V1",
            "Intake Form",
            "Initial patient intake",
            "{\"type\":\"object\"}"
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(template.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var query = new GetClinicalFormTemplateByIdQuery(template.Id);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDto = new ClinicalFormTemplateDto(
            template.Id,
            template.Code,
            template.Name,
            template.Description,
            template.JsonSchemaDefinition,
            template.IsDeleted
        );

        result.Should().BeEquivalentTo(expectedDto);

        _repositoryMock.Verify(
            x => x.GetByIdAsync(template.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenTemplateDoesNotExist()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        _repositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClinicalFormTemplate?)null);

        var query = new GetClinicalFormTemplateByIdQuery(id);

        // Act
        var act = async () => await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(ClinicalFormTemplate));

        _repositoryMock.Verify(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
