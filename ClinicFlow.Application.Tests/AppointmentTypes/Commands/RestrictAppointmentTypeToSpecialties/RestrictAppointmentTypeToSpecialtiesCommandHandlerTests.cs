using AwesomeAssertions;
using ClinicFlow.Application.AppointmentTypes.Commands.RestrictAppointmentTypeToSpecialties;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Moq;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Commands.RestrictAppointmentTypeToSpecialties;

public class RestrictAppointmentTypeToSpecialtiesCommandHandlerTests
{
    private readonly Mock<IAppointmentTypeDefinitionRepository> _appointmentTypeRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RestrictAppointmentTypeToSpecialtiesCommandHandler _sut;

    public RestrictAppointmentTypeToSpecialtiesCommandHandlerTests()
    {
        _appointmentTypeRepositoryMock = new Mock<IAppointmentTypeDefinitionRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new RestrictAppointmentTypeToSpecialtiesCommandHandler(
            _appointmentTypeRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldRestrictToSpecialties_WhenEntityExistsAndIsUnrestricted()
    {
        // Arrange
        var specialtyIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var command = new RestrictAppointmentTypeToSpecialtiesCommand(Guid.NewGuid(), specialtyIds);
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Description",
            EncounterDuration.FromMinutes(30)
        );

        _appointmentTypeRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointmentType);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        appointmentType.IsUnrestrictedBySpecialty.Should().BeFalse();
        appointmentType.AllowedSpecialtyIds.Should().BeEquivalentTo(specialtyIds);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenEntityDoesNotExist()
    {
        // Arrange
        var command = new RestrictAppointmentTypeToSpecialtiesCommand(
            Guid.NewGuid(),
            [Guid.NewGuid()]
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
