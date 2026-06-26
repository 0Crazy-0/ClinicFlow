using AwesomeAssertions;
using ClinicFlow.Application.ClinicalFormTemplates.Commands.DeactivateClinicalFormTemplate;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using Moq;

namespace ClinicFlow.Application.Tests.ClinicalFormTemplates.Commands.DeactivateClinicalFormTemplate;

public class DeactivateClinicalFormTemplateCommandHandlerTests
{
    private readonly Mock<IClinicalFormTemplateRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeactivateClinicalFormTemplateCommandHandler _sut;

    public DeactivateClinicalFormTemplateCommandHandlerTests()
    {
        _repositoryMock = new Mock<IClinicalFormTemplateRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new DeactivateClinicalFormTemplateCommandHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldDeactivateTemplate_WhenActive()
    {
        // Arrange
        var template = ClinicalFormTemplate.Create(
            "CARDIO_01",
            "Cardiology Form",
            "For cardiac evaluations",
            "{}"
        );
        var command = new DeactivateClinicalFormTemplateCommand(template.Id);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(command.TemplateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        template.IsDeleted.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenNotFound()
    {
        // Arrange
        var command = new DeactivateClinicalFormTemplateCommand(Guid.NewGuid());

        _repositoryMock
            .Setup(x => x.GetByIdAsync(command.TemplateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClinicalFormTemplate?)null);

        // Act
        var act = () => _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(ClinicalFormTemplate));

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
