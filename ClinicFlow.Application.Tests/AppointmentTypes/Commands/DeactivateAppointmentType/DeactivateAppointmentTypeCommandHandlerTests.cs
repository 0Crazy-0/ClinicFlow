using AwesomeAssertions;
using ClinicFlow.Application.AppointmentTypes.Commands.DeactivateAppointmentType;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Moq;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Commands.DeactivateAppointmentType;

public class DeactivateAppointmentTypeCommandHandlerTests
{
    private readonly Mock<IAppointmentTypeDefinitionRepository> _appointmentTypeRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeactivateAppointmentTypeCommandHandler _sut;

    public DeactivateAppointmentTypeCommandHandlerTests()
    {
        _appointmentTypeRepositoryMock = new Mock<IAppointmentTypeDefinitionRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new DeactivateAppointmentTypeCommandHandler(
            _appointmentTypeRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldDeactivateAppointmentType_WhenActive()
    {
        // Arrange
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "General Checkup",
            "Routine consultation",
            EncounterDuration.FromMinutes(30)
        );

        var command = new DeactivateAppointmentTypeCommand(appointmentType.Id);

        _appointmentTypeRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointmentType);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        appointmentType.IsDeleted.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenNotFound()
    {
        // Arrange
        var command = new DeactivateAppointmentTypeCommand(Guid.CreateVersion7());

        _appointmentTypeRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppointmentTypeDefinition?)null);

        // Act
        var act = () => _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(AppointmentTypeDefinition));

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
