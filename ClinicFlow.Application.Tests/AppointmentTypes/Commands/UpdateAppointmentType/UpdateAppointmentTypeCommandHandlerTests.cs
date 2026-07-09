using AwesomeAssertions;
using ClinicFlow.Application.AppointmentTypes.Commands.UpdateAppointmentType;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Moq;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Commands.UpdateAppointmentType;

public class UpdateAppointmentTypeCommandHandlerTests
{
    private readonly Mock<IAppointmentTypeDefinitionRepository> _appointmentTypeRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateAppointmentTypeCommandHandler _sut;

    public UpdateAppointmentTypeCommandHandlerTests()
    {
        _appointmentTypeRepositoryMock = new Mock<IAppointmentTypeDefinitionRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new UpdateAppointmentTypeCommandHandler(
            _appointmentTypeRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldUpdateAppointmentType_WhenEntityExists()
    {
        // Arrange
        var command = new UpdateAppointmentTypeCommand(
            Guid.CreateVersion7(),
            AppointmentCategory.FollowUp,
            "Updated Checkup",
            "Updated description",
            45
        );

        var existingEntity = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Original Checkup",
            "Original description",
            EncounterDuration.FromMinutes(30)
        );

        _appointmentTypeRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        existingEntity.Category.Should().Be(command.Category);
        existingEntity.Name.Should().Be(command.Name);
        existingEntity.Description.Should().Be(command.Description);
        existingEntity.Duration.Minutes.Should().Be(command.DurationMinutes);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenEntityDoesNotExist()
    {
        // Arrange
        var command = new UpdateAppointmentTypeCommand(
            Guid.CreateVersion7(),
            AppointmentCategory.Checkup,
            "Checkup",
            "Description",
            30
        );

        _appointmentTypeRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppointmentTypeDefinition?)null);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

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
        var command = new UpdateAppointmentTypeCommand(
            Guid.CreateVersion7(),
            AppointmentCategory.Checkup,
            "Existing Name",
            "Description",
            30
        );

        var existingEntity = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Original Name",
            "Original description",
            EncounterDuration.FromMinutes(30)
        );

        _appointmentTypeRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        _appointmentTypeRepositoryMock
            .Setup(x =>
                x.ExistsByNameExcludingAsync(
                    command.Name,
                    command.AppointmentTypeId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.AppointmentType.NameAlreadyExists);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
