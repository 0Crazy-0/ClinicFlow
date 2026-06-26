using AwesomeAssertions;
using ClinicFlow.Application.Doctors.Queries.GetDoctorByUserId;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Moq;

namespace ClinicFlow.Application.Tests.Doctors.Queries.GetDoctorByUserId;

public class GetDoctorByUserIdQueryHandlerTests
{
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
    private readonly GetDoctorByUserIdQueryHandler _sut;

    public GetDoctorByUserIdQueryHandlerTests()
    {
        _doctorRepositoryMock = new Mock<IDoctorRepository>();
        _sut = new GetDoctorByUserIdQueryHandler(_doctorRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDoctor_WhenDoctorExistsForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var doctor = Doctor.Create(
            userId,
            PersonName.Create("Test Doctor"),
            MedicalLicenseNumber.Create("67890"),
            Guid.NewGuid(),
            "Dermatologist specialized in skin cancer detection",
            ConsultationRoom.Create(2, "Dermatology B", 5)
        );

        _doctorRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        // Act
        var result = await _sut.Handle(
            new GetDoctorByUserIdQuery(userId),
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(doctor.Id);
        result.UserId.Should().Be(userId);
        result.FullName.Should().Be(doctor.FullName.FullName);
        result.LicenseNumber.Should().Be(doctor.LicenseNumber.Value);
        result.Biography.Should().Be(doctor.Biography);
        result.ConsultationRoomNumber.Should().Be(doctor.ConsultationRoom.Number);
        result.ConsultationRoomName.Should().Be(doctor.ConsultationRoom.Name);
        result.ConsultationRoomFloor.Should().Be(doctor.ConsultationRoom.Floor);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenDoctorDoesNotExistForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _doctorRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        // Act
        var act = async () =>
            await _sut.Handle(
                new GetDoctorByUserIdQuery(userId),
                TestContext.Current.CancellationToken
            );

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Doctor));

        _doctorRepositoryMock.Verify(
            x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
