using ClinicFlow.Application.AppointmentTypes.Commands.CreateAppointmentType;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Commands.CreateAppointmentType;

public class CreateAppointmentTypeCommandHandlerTests
{
    private readonly Mock<IAppointmentTypeDefinitionRepository> _appointmentTypeRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateAppointmentTypeCommandHandler _sut;

    public CreateAppointmentTypeCommandHandlerTests()
    {
        _appointmentTypeRepositoryMock = new Mock<IAppointmentTypeDefinitionRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new CreateAppointmentTypeCommandHandler(
            _appointmentTypeRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateAppointmentType_WhenValidCommand()
    {
        // Arrange
        var command = new CreateAppointmentTypeCommand(
            AppointmentCategory.Checkup,
            "General Checkup",
            "Routine consultation",
            TimeSpan.FromMinutes(30),
            18,
            65,
            false
        );

        AppointmentTypeDefinition? capturedEntity = null;
        _appointmentTypeRepositoryMock
            .Setup(x =>
                x.CreateAsync(It.IsAny<AppointmentTypeDefinition>(), It.IsAny<CancellationToken>())
            )
            .Callback<AppointmentTypeDefinition, CancellationToken>(
                (entity, _) => capturedEntity = entity
            );

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        capturedEntity.Should().NotBeNull();
        capturedEntity!.Category.Should().Be(command.Category);
        capturedEntity.Name.Should().Be(command.Name);
        capturedEntity.Description.Should().Be(command.Description);
        capturedEntity.DurationMinutes.Should().Be(command.DurationMinutes);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryCreateAndSaveChanges_WhenValidCommand()
    {
        // Arrange
        var command = new CreateAppointmentTypeCommand(
            AppointmentCategory.Checkup,
            "General Checkup",
            "Routine consultation",
            TimeSpan.FromMinutes(30),
            null,
            null,
            false
        );

        _appointmentTypeRepositoryMock
            .Setup(x =>
                x.CreateAsync(It.IsAny<AppointmentTypeDefinition>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((AppointmentTypeDefinition entity, CancellationToken _) => entity);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _appointmentTypeRepositoryMock.Verify(
            x =>
                x.CreateAsync(It.IsAny<AppointmentTypeDefinition>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
