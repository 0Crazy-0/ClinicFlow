using ClinicFlow.Application.Doctors.Commands.CreateDoctorProfile;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Doctors.Commands.CreateDoctorProfile;

public class CreateDoctorProfileCommandHandlerTests
{
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateDoctorProfileCommandHandler _sut;

    public CreateDoctorProfileCommandHandlerTests()
    {
        _doctorRepositoryMock = new Mock<IDoctorRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new CreateDoctorProfileCommandHandler(
            _doctorRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateDoctorProfile_WhenValidCommand()
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.NewGuid(),
            "12345",
            Guid.NewGuid(),
            "Cardiologist with 10 years of experience",
            101
        );

        Doctor? capturedDoctor = null;
        _doctorRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Doctor>(), It.IsAny<CancellationToken>()))
            .Callback<Doctor, CancellationToken>((d, _) => capturedDoctor = d);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        capturedDoctor.Should().NotBeNull();
        capturedDoctor!.UserId.Should().Be(command.UserId);
        capturedDoctor.LicenseNumber.Value.Should().Be(command.LicenseNumber);
        capturedDoctor.MedicalSpecialtyId.Should().Be(command.MedicalSpecialtyId);
        capturedDoctor.Biography.Should().Be(command.Biography);
        capturedDoctor.ConsultationRoomNumber.Should().Be(command.ConsultationRoomNumber);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryCreateAndSaveChanges_WhenValidCommand()
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.NewGuid(),
            "12345",
            Guid.NewGuid(),
            "Biography",
            101
        );

        _doctorRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Doctor>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor d, CancellationToken _) => d);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _doctorRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<Doctor>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
