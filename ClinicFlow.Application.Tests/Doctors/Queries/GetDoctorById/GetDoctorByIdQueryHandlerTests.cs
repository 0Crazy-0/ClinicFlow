using ClinicFlow.Application.Doctors.Queries.GetDoctorById;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Doctors.Queries.GetDoctorById;

public class GetDoctorByIdQueryHandlerTests
{
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
    private readonly GetDoctorByIdQueryHandler _sut;

    public GetDoctorByIdQueryHandlerTests()
    {
        _doctorRepositoryMock = new Mock<IDoctorRepository>();
        _sut = new GetDoctorByIdQueryHandler(_doctorRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDoctor_WhenDoctorExists()
    {
        // Arrange
        var doctor = Doctor.Create(
            Guid.NewGuid(),
            MedicalLicenseNumber.Create("12345"),
            Guid.NewGuid(),
            "Cardiologist with 10 years of experience",
            ConsultationRoom.Create(1, "Cardiology A", 3)
        );

        _doctorRepositoryMock
            .Setup(x => x.GetByIdAsync(doctor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        // Act
        var result = await _sut.Handle(new GetDoctorByIdQuery(doctor.Id), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(doctor.Id);
        result.UserId.Should().Be(doctor.UserId);
        result.MedicalSpecialtyId.Should().Be(doctor.MedicalSpecialtyId);
        result.LicenseNumber.Should().Be("12345");
        result.Biography.Should().Be("Cardiologist with 10 years of experience");
        result.ConsultationRoomNumber.Should().Be(1);
        result.ConsultationRoomName.Should().Be("Cardiology A");
        result.ConsultationRoomFloor.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenDoctorDoesNotExist()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        _doctorRepositoryMock
            .Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        // Act
        var act = async () =>
            await _sut.Handle(new GetDoctorByIdQuery(doctorId), CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Doctor));
    }
}
