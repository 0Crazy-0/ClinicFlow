using ClinicFlow.Application.Doctors.Queries.GetDoctorsBySpecialtyId;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Doctors.Queries.GetDoctorsBySpecialtyId;

public class GetDoctorsBySpecialtyIdQueryHandlerTests
{
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
    private readonly GetDoctorsBySpecialtyIdQueryHandler _sut;

    public GetDoctorsBySpecialtyIdQueryHandlerTests()
    {
        _doctorRepositoryMock = new Mock<IDoctorRepository>();
        _sut = new GetDoctorsBySpecialtyIdQueryHandler(_doctorRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDoctors_WhenDoctorsExistForSpecialty()
    {
        // Arrange
        var specialtyId = Guid.NewGuid();
        var doctor1 = Doctor.Create(
            Guid.NewGuid(),
            MedicalLicenseNumber.Create("12345"),
            specialtyId,
            "Cardiologist with 10 years of experience",
            ConsultationRoom.Create(1, "Cardiology A", 3)
        );
        var doctor2 = Doctor.Create(
            Guid.NewGuid(),
            MedicalLicenseNumber.Create("67890"),
            specialtyId,
            "Cardiologist specialized in arrhythmias",
            ConsultationRoom.Create(2, "Cardiology B", 3)
        );

        var doctors = new List<Doctor> { doctor1, doctor2 };

        _doctorRepositoryMock
            .Setup(x => x.GetBySpecialtyIdAsync(specialtyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctors);

        // Act
        var result = await _sut.Handle(
            new GetDoctorsBySpecialtyIdQuery(specialtyId),
            CancellationToken.None
        );

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var resultList = result.ToList();
        resultList[0].LicenseNumber.Should().Be("12345");
        resultList[0].MedicalSpecialtyId.Should().Be(specialtyId);
        resultList[0].Biography.Should().Be("Cardiologist with 10 years of experience");
        resultList[0].ConsultationRoomNumber.Should().Be(1);
        resultList[0].ConsultationRoomName.Should().Be("Cardiology A");
        resultList[0].ConsultationRoomFloor.Should().Be(3);

        resultList[1].LicenseNumber.Should().Be("67890");
        resultList[1].MedicalSpecialtyId.Should().Be(specialtyId);
        resultList[1].Biography.Should().Be("Cardiologist specialized in arrhythmias");
        resultList[1].ConsultationRoomNumber.Should().Be(2);
        resultList[1].ConsultationRoomName.Should().Be("Cardiology B");
        resultList[1].ConsultationRoomFloor.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoDoctorsExistForSpecialty()
    {
        // Arrange
        var specialtyId = Guid.NewGuid();

        _doctorRepositoryMock
            .Setup(x => x.GetBySpecialtyIdAsync(specialtyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _sut.Handle(
            new GetDoctorsBySpecialtyIdQuery(specialtyId),
            CancellationToken.None
        );

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
