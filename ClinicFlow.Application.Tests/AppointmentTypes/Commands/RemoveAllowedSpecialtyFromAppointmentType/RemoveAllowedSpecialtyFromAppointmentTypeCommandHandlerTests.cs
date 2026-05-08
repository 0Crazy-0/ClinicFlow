using ClinicFlow.Application.AppointmentTypes.Commands.RemoveAllowedSpecialtyFromAppointmentType;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Commands.RemoveAllowedSpecialtyFromAppointmentType;

public class RemoveAllowedSpecialtyFromAppointmentTypeCommandHandlerTests
{
    private readonly Mock<IAppointmentTypeDefinitionRepository> _appointmentTypeRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RemoveAllowedSpecialtyFromAppointmentTypeCommandHandler _sut;

    public RemoveAllowedSpecialtyFromAppointmentTypeCommandHandlerTests()
    {
        _appointmentTypeRepositoryMock = new Mock<IAppointmentTypeDefinitionRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new RemoveAllowedSpecialtyFromAppointmentTypeCommandHandler(
            _appointmentTypeRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldRemoveSpecialty_WhenEntityExistsAndSpecialtyIsAllowed()
    {
        // Arrange
        var specialtyToRemove = Guid.NewGuid();
        var remainingSpecialty = Guid.NewGuid();
        var command = new RemoveAllowedSpecialtyFromAppointmentTypeCommand(
            Guid.NewGuid(),
            specialtyToRemove
        );

        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Description",
            TimeSpan.FromMinutes(30)
        );

        appointmentType.RestrictToSpecialties([specialtyToRemove, remainingSpecialty]);

        _appointmentTypeRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointmentType);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        appointmentType.AllowedSpecialtyIds.Should().NotContain(specialtyToRemove);
        appointmentType
            .AllowedSpecialtyIds.Should()
            .ContainSingle()
            .Which.Should()
            .Be(remainingSpecialty);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenEntityDoesNotExist()
    {
        // Arrange
        var command = new RemoveAllowedSpecialtyFromAppointmentTypeCommand(
            Guid.NewGuid(),
            Guid.NewGuid()
        );

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

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
