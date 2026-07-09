using AwesomeAssertions;
using ClinicFlow.Application.AppointmentTypes.Commands.AddAllowedSpecialtyToAppointmentType;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Moq;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Commands.AddAllowedSpecialtyToAppointmentType;

public class AddAllowedSpecialtyToAppointmentTypeCommandHandlerTests
{
    private readonly Mock<IAppointmentTypeDefinitionRepository> _appointmentTypeRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AddAllowedSpecialtyToAppointmentTypeCommandHandler _sut;

    public AddAllowedSpecialtyToAppointmentTypeCommandHandlerTests()
    {
        _appointmentTypeRepositoryMock = new Mock<IAppointmentTypeDefinitionRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new AddAllowedSpecialtyToAppointmentTypeCommandHandler(
            _appointmentTypeRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldAddSpecialty_WhenEntityExistsAndIsRestricted()
    {
        // Arrange
        var existingSpecialtyId = Guid.CreateVersion7();
        var newSpecialtyId = Guid.CreateVersion7();
        var command = new AddAllowedSpecialtyToAppointmentTypeCommand(
            Guid.CreateVersion7(),
            newSpecialtyId
        );

        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Description",
            EncounterDuration.FromMinutes(30)
        );

        appointmentType.RestrictToSpecialties([existingSpecialtyId]);

        _appointmentTypeRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointmentType);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        appointmentType.AllowedSpecialtyIds.Should().Contain(newSpecialtyId);
        appointmentType.AllowedSpecialtyIds.Should().HaveCount(2);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenEntityDoesNotExist()
    {
        // Arrange
        var command = new AddAllowedSpecialtyToAppointmentTypeCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7()
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
}
