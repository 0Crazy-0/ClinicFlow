using ClinicFlow.Application.ClinicalFormTemplates.Commands.CreateClinicalFormTemplate;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.ClinicalFormTemplates.Commands.CreateClinicalFormTemplate;

public class CreateClinicalFormTemplateCommandHandlerTests
{
    private readonly Mock<IClinicalFormTemplateRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateClinicalFormTemplateCommandHandler _sut;

    public CreateClinicalFormTemplateCommandHandlerTests()
    {
        _repositoryMock = new Mock<IClinicalFormTemplateRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new CreateClinicalFormTemplateCommandHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateTemplate_WhenValidCommand()
    {
        // Arrange
        var command = new CreateClinicalFormTemplateCommand(
            "CARDIO_01",
            "Cardiology Form",
            "For cardiac evaluations",
            """{\"fields\":["heartRate"]}"""
        );

        ClinicalFormTemplate? capturedTemplate = null;
        _repositoryMock
            .Setup(x =>
                x.CreateAsync(It.IsAny<ClinicalFormTemplate>(), It.IsAny<CancellationToken>())
            )
            .Callback<ClinicalFormTemplate, CancellationToken>((t, _) => capturedTemplate = t);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        capturedTemplate.Should().NotBeNull();
        capturedTemplate.Code.Should().Be(command.Code);
        capturedTemplate.Name.Should().Be(command.Name);
        capturedTemplate.Description.Should().Be(command.Description);
        capturedTemplate.JsonSchemaDefinition.Should().Be(command.JsonSchemaDefinition);
    }

    [Fact]
    public async Task Handle_ShouldCreateTemplateAndSaveChanges_WhenValidCommand()
    {
        // Arrange
        var command = new CreateClinicalFormTemplateCommand(
            "TEMPLATE_01",
            "Template",
            "Description",
            "{}"
        );

        _repositoryMock
            .Setup(x =>
                x.CreateAsync(It.IsAny<ClinicalFormTemplate>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((ClinicalFormTemplate t, CancellationToken _) => t);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<ClinicalFormTemplate>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenCodeAlreadyExists()
    {
        // Arrange
        var command = new CreateClinicalFormTemplateCommand(
            "CARDIO_01",
            "Cardiology Form",
            "For cardiac evaluations",
            "{}"
        );

        _repositoryMock
            .Setup(x => x.ExistsByCodeAsync(command.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.ClinicalFormTemplate.CodeAlreadyExists);

        _repositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<ClinicalFormTemplate>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenNameAlreadyExists()
    {
        // Arrange
        var command = new CreateClinicalFormTemplateCommand(
            "NEW_CODE",
            "Existing Name",
            "Description",
            "{}"
        );

        _repositoryMock
            .Setup(x => x.ExistsByNameAsync(command.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.ClinicalFormTemplate.NameAlreadyExists);

        _repositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<ClinicalFormTemplate>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
