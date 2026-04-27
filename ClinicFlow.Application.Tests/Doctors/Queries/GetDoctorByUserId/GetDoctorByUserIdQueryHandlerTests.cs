using ClinicFlow.Application.Doctors.Queries.GetDoctorByUserId;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
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
            MedicalLicenseNumber.Create("67890"),
            Guid.NewGuid(),
            "Dermatologist specialized in skin cancer detection",
            ConsultationRoom.Create(2, "Dermatology B", 5)
        );

        _doctorRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        // Act
        var result = await _sut.Handle(new GetDoctorByUserIdQuery(userId), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(doctor.Id);
        result.UserId.Should().Be(userId);
        result.LicenseNumber.Should().Be("67890");
        result.Biography.Should().Be("Dermatologist specialized in skin cancer detection");
        result.ConsultationRoomNumber.Should().Be(2);
        result.ConsultationRoomName.Should().Be("Dermatology B");
        result.ConsultationRoomFloor.Should().Be(5);
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
            await _sut.Handle(new GetDoctorByUserIdQuery(userId), CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Doctor));
    }
}
