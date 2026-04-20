using ClinicFlow.Application.ClinicalFormTemplates.Commands.DeleteClinicalFormTemplate;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.ClinicalFormTemplates.Commands.DeleteClinicalFormTemplate;

public class DeleteClinicalFormTemplateCommandHandlerTests
{
    private readonly Mock<IClinicalFormTemplateRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteClinicalFormTemplateCommandHandler _sut;

    public DeleteClinicalFormTemplateCommandHandlerTests()
    {
        _repositoryMock = new Mock<IClinicalFormTemplateRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new DeleteClinicalFormTemplateCommandHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldMarkAsDeleted_WhenEntityExists()
    {
        // Arrange
        var existingTemplate = ClinicalFormTemplate.Create(
            "TEMPLATE_01",
            "Template",
            "Description",
            "{}"
        );

        var command = new DeleteClinicalFormTemplateCommand(existingTemplate.Id);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(command.TemplateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTemplate);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        existingTemplate.IsDeleted.Should().BeTrue();

        _repositoryMock.Verify(
            x => x.UpdateAsync(existingTemplate, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenEntityDoesNotExist()
    {
        // Arrange
        var command = new DeleteClinicalFormTemplateCommand(Guid.NewGuid());

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

        _repositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<ClinicalFormTemplate>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
