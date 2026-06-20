using AwesomeAssertions;
using ClinicFlow.Application.AppointmentTypes.Commands.ReactivateAppointmentType;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Moq;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Commands.ReactivateAppointmentType;

public class ReactivateAppointmentTypeCommandHandlerTests
{
    private readonly Mock<IAppointmentTypeDefinitionRepository> _appointmentTypeRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ReactivateAppointmentTypeCommandHandler _sut;

    public ReactivateAppointmentTypeCommandHandlerTests()
    {
        _appointmentTypeRepositoryMock = new Mock<IAppointmentTypeDefinitionRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new ReactivateAppointmentTypeCommandHandler(
            _appointmentTypeRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReactivateAppointmentType_WhenInactive()
    {
        // Arrange
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "General Checkup",
            "Routine consultation",
            EncounterDuration.FromMinutes(30)
        );
        appointmentType.Deactivate();

        var command = new ReactivateAppointmentTypeCommand(appointmentType.Id);

        _appointmentTypeRepositoryMock
            .Setup(x =>
                x.GetByIdIncludingDeletedAsync(
                    command.AppointmentTypeId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(appointmentType);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        appointmentType.IsDeleted.Should().BeFalse();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenNotFound()
    {
        // Arrange
        var command = new ReactivateAppointmentTypeCommand(Guid.NewGuid());

        _appointmentTypeRepositoryMock
            .Setup(x =>
                x.GetByIdIncludingDeletedAsync(
                    command.AppointmentTypeId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((AppointmentTypeDefinition?)null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(AppointmentTypeDefinition));

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenNameAlreadyExists()
    {
        // Arrange
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "General Checkup",
            "Routine consultation",
            EncounterDuration.FromMinutes(30)
        );
        appointmentType.Deactivate();

        var command = new ReactivateAppointmentTypeCommand(appointmentType.Id);

        _appointmentTypeRepositoryMock
            .Setup(x =>
                x.GetByIdIncludingDeletedAsync(
                    command.AppointmentTypeId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(appointmentType);

        _appointmentTypeRepositoryMock
            .Setup(x => x.ExistsByNameAsync(appointmentType.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.AppointmentType.NameAlreadyExists);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
