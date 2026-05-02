using ClinicFlow.Application.ClinicalFormTemplates.Commands.UpdateClinicalFormTemplate;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.ClinicalFormTemplates.Commands.UpdateClinicalFormTemplate;

public class UpdateClinicalFormTemplateCommandHandlerTests
{
    private readonly Mock<IClinicalFormTemplateRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateClinicalFormTemplateCommandHandler _sut;

    public UpdateClinicalFormTemplateCommandHandlerTests()
    {
        _repositoryMock = new Mock<IClinicalFormTemplateRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new UpdateClinicalFormTemplateCommandHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldUpdateTemplate_WhenEntityExists()
    {
        // Arrange
        var existingTemplate = ClinicalFormTemplate.Create(
            "CARDIO_01",
            "Old Name",
            "Old Description",
            """{"fields":[]}"""
        );

        var command = new UpdateClinicalFormTemplateCommand(
            existingTemplate.Id,
            "Updated Name",
            "Updated Description",
            """{"fields":["heartRate"]}"""
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(command.TemplateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTemplate);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        existingTemplate.Name.Should().Be(command.Name);
        existingTemplate.Description.Should().Be(command.Description);
        existingTemplate.JsonSchemaDefinition.Should().Be(command.JsonSchemaDefinition);
        existingTemplate.Code.Should().Be("CARDIO_01");

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenEntityDoesNotExist()
    {
        // Arrange
        var command = new UpdateClinicalFormTemplateCommand(
            Guid.NewGuid(),
            "Name",
            "Description",
            "{}"
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(command.TemplateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClinicalFormTemplate?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(ClinicalFormTemplate));
    }
}
