using ClinicFlow.Application.AppointmentTypes.Commands.DeleteAppointmentType;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Commands.DeleteAppointmentType;

public class DeleteAppointmentTypeCommandHandlerTests
{
    private readonly Mock<IAppointmentTypeDefinitionRepository> _appointmentTypeRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteAppointmentTypeCommandHandler _sut;

    public DeleteAppointmentTypeCommandHandlerTests()
    {
        _appointmentTypeRepositoryMock = new Mock<IAppointmentTypeDefinitionRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new DeleteAppointmentTypeCommandHandler(
            _appointmentTypeRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldMarkAsDeleted_WhenEntityExists()
    {
        // Arrange
        var command = new DeleteAppointmentTypeCommand(Guid.NewGuid());

        var existingEntity = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Description",
            TimeSpan.FromMinutes(30)
        );

        _appointmentTypeRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        existingEntity.IsDeleted.Should().BeTrue();

        _appointmentTypeRepositoryMock.Verify(
            x => x.UpdateAsync(existingEntity, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenEntityDoesNotExist()
    {
        // Arrange
        var command = new DeleteAppointmentTypeCommand(Guid.NewGuid());

        _appointmentTypeRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppointmentTypeDefinition?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(AppointmentTypeDefinition));

        _appointmentTypeRepositoryMock.Verify(
            x =>
                x.UpdateAsync(It.IsAny<AppointmentTypeDefinition>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
