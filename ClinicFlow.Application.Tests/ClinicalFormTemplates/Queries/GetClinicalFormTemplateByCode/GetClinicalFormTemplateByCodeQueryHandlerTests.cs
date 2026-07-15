using AwesomeAssertions;
using ClinicFlow.Application.ClinicalFormTemplates.Queries.DTOs;
using ClinicFlow.Application.ClinicalFormTemplates.Queries.GetClinicalFormTemplateByCode;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using Moq;

namespace ClinicFlow.Application.Tests.ClinicalFormTemplates.Queries.GetClinicalFormTemplateByCode;

public class GetClinicalFormTemplateByCodeQueryHandlerTests
{
    private readonly Mock<IClinicalFormTemplateRepository> _repositoryMock;
    private readonly GetClinicalFormTemplateByCodeQueryHandler _sut;

    public GetClinicalFormTemplateByCodeQueryHandlerTests()
    {
        _repositoryMock = new Mock<IClinicalFormTemplateRepository>();
        _sut = new GetClinicalFormTemplateByCodeQueryHandler(_repositoryMock.Object);
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
            .Setup(x => x.GetByCodeAsync(template.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var query = new GetClinicalFormTemplateByCodeQuery(template.Code);

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
            x => x.GetByCodeAsync(template.Code, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenTemplateDoesNotExist()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByCodeAsync("NONEXISTENT", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClinicalFormTemplate?)null);

        var query = new GetClinicalFormTemplateByCodeQuery("NONEXISTENT");

        // Act
        var act = async () => await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(ClinicalFormTemplate));

        _repositoryMock.Verify(
            x => x.GetByCodeAsync("NONEXISTENT", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
