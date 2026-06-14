using AwesomeAssertions;
using ClinicFlow.Application.Doctors.Queries.GetDoctorsBySpecialtyId;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
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
    public async Task Handle_ShouldReturnPaginatedList_WhenDoctorsExistForSpecialty()
    {
        // Arrange
        var specialtyId = Guid.NewGuid();
        var doctor1 = Doctor.Create(
            Guid.NewGuid(),
            PersonName.Create("Test Doctor"),
            MedicalLicenseNumber.Create("12345"),
            specialtyId,
            "Cardiologist with 10 years of experience",
            ConsultationRoom.Create(1, "Cardiology A", 3)
        );

        var doctor2 = Doctor.Create(
            Guid.NewGuid(),
            PersonName.Create("Test Doctor"),
            MedicalLicenseNumber.Create("67890"),
            specialtyId,
            "Cardiologist specialized in arrhythmias",
            ConsultationRoom.Create(2, "Cardiology B", 3)
        );

        _doctorRepositoryMock
            .Setup(x =>
                x.GetBySpecialtyIdPaginatedAsync(specialtyId, 1, 10, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(([doctor1, doctor2], 2));

        // Act
        var result = await _sut.Handle(
            new GetDoctorsBySpecialtyIdQuery(specialtyId, 1, 10),
            CancellationToken.None
        );

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.Items.Should().HaveCount(2);

        var resultList = result.Items.ToList();
        resultList[0].Id.Should().Be(doctor1.Id);
        resultList[0].FullName.Should().Be(doctor1.FullName.FullName);
        resultList[0].LicenseNumber.Should().Be(doctor1.LicenseNumber.Value);
        resultList[0].MedicalSpecialtyId.Should().Be(specialtyId);
        resultList[1].Id.Should().Be(doctor2.Id);
        resultList[1].FullName.Should().Be(doctor2.FullName.FullName);
        resultList[1].LicenseNumber.Should().Be(doctor2.LicenseNumber.Value);
        resultList[1].MedicalSpecialtyId.Should().Be(specialtyId);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPaginatedList_WhenNoDoctorsExistForSpecialty()
    {
        // Arrange
        var specialtyId = Guid.NewGuid();

        _doctorRepositoryMock
            .Setup(x =>
                x.GetBySpecialtyIdPaginatedAsync(specialtyId, 1, 10, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((new List<Doctor>(), 0));

        // Act
        var result = await _sut.Handle(
            new GetDoctorsBySpecialtyIdQuery(specialtyId, 1, 10),
            CancellationToken.None
        );

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }
}
