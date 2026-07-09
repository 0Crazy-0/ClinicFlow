using AwesomeAssertions;
using ClinicFlow.Application.ClinicalFormTemplates.Commands.UpdateClinicalFormTemplate;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
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
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        existingTemplate.Name.Should().Be(command.Name);
        existingTemplate.Description.Should().Be(command.Description);
        existingTemplate.JsonSchemaDefinition.Should().Be(command.JsonSchemaDefinition);
        existingTemplate.Code.Should().Be("CARDIO_01");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenEntityDoesNotExist()
    {
        // Arrange
        var command = new UpdateClinicalFormTemplateCommand(
            Guid.CreateVersion7(),
            "Name",
            "Description",
            "{}"
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(command.TemplateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClinicalFormTemplate?)null);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(ClinicalFormTemplate));

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenNameAlreadyExists()
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
            "Existing Name",
            "Updated Description",
            """{"fields":["heartRate"]}"""
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(command.TemplateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTemplate);

        _repositoryMock
            .Setup(x =>
                x.ExistsByNameExcludingAsync(
                    command.Name,
                    command.TemplateId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.ClinicalFormTemplate.NameAlreadyExists);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
