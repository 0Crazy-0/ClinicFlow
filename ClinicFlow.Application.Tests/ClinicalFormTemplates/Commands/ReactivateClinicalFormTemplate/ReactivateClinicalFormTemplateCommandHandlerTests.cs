using AwesomeAssertions;
using ClinicFlow.Application.ClinicalFormTemplates.Commands.ReactivateClinicalFormTemplate;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using Moq;

namespace ClinicFlow.Application.Tests.ClinicalFormTemplates.Commands.ReactivateClinicalFormTemplate;

public class ReactivateClinicalFormTemplateCommandHandlerTests
{
    private readonly Mock<IClinicalFormTemplateRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ReactivateClinicalFormTemplateCommandHandler _sut;

    public ReactivateClinicalFormTemplateCommandHandlerTests()
    {
        _repositoryMock = new Mock<IClinicalFormTemplateRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new ReactivateClinicalFormTemplateCommandHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReactivateTemplate_WhenInactive()
    {
        // Arrange
        var template = ClinicalFormTemplate.Create(
            "CARDIO_01",
            "Cardiology Form",
            "For cardiac evaluations",
            "{}"
        );
        template.Deactivate();

        var command = new ReactivateClinicalFormTemplateCommand(template.Id);

        _repositoryMock
            .Setup(x =>
                x.GetByIdIncludingDeletedAsync(command.TemplateId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(template);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        template.IsDeleted.Should().BeFalse();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenNotFound()
    {
        // Arrange
        var command = new ReactivateClinicalFormTemplateCommand(Guid.CreateVersion7());

        _repositoryMock
            .Setup(x =>
                x.GetByIdIncludingDeletedAsync(command.TemplateId, It.IsAny<CancellationToken>())
            )
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

    [Fact]
    public async Task Handle_ShouldThrowException_WhenCodeAlreadyExists()
    {
        // Arrange
        var template = ClinicalFormTemplate.Create(
            "CARDIO_01",
            "Cardiology Form",
            "For cardiac evaluations",
            "{}"
        );
        template.Deactivate();

        var command = new ReactivateClinicalFormTemplateCommand(template.Id);

        _repositoryMock
            .Setup(x =>
                x.GetByIdIncludingDeletedAsync(command.TemplateId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(template);

        _repositoryMock
            .Setup(x => x.ExistsByCodeAsync(template.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.ClinicalFormTemplate.CodeAlreadyExists);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenNameAlreadyExists()
    {
        // Arrange
        var template = ClinicalFormTemplate.Create(
            "CARDIO_01",
            "Cardiology Form",
            "For cardiac evaluations",
            "{}"
        );
        template.Deactivate();

        var command = new ReactivateClinicalFormTemplateCommand(template.Id);

        _repositoryMock
            .Setup(x =>
                x.GetByIdIncludingDeletedAsync(command.TemplateId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(template);

        _repositoryMock
            .Setup(x => x.ExistsByNameAsync(template.Name, It.IsAny<CancellationToken>()))
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
