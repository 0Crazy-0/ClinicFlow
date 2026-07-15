using AwesomeAssertions;
using ClinicFlow.Application.Doctors.Queries.DTOs;
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
        var specialtyId = Guid.CreateVersion7();
        var doctor1 = Doctor.Create(
            Guid.CreateVersion7(),
            PersonName.Create("Test Doctor"),
            MedicalLicenseNumber.Create("12345"),
            specialtyId,
            "Cardiologist with 10 years of experience",
            ConsultationRoom.Create(1, "Cardiology A", 3)
        );

        var doctor2 = Doctor.Create(
            Guid.CreateVersion7(),
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
            TestContext.Current.CancellationToken
        );

        // Assert
        var expectedDtos = new List<Doctor> { doctor1, doctor2 }.Select(doctor => new DoctorDto(
            doctor.Id,
            doctor.UserId,
            doctor.FullName.FullName,
            doctor.MedicalSpecialtyId,
            doctor.LicenseNumber.Value,
            doctor.Biography,
            doctor.ConsultationRoom.Number,
            doctor.ConsultationRoom.Name,
            doctor.ConsultationRoom.Floor
        ));

        result.Items.Should().BeEquivalentTo(expectedDtos);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(1);

        _doctorRepositoryMock.Verify(
            x =>
                x.GetBySpecialtyIdPaginatedAsync(specialtyId, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPaginatedList_WhenNoDoctorsExistForSpecialty()
    {
        // Arrange
        var specialtyId = Guid.CreateVersion7();

        _doctorRepositoryMock
            .Setup(x =>
                x.GetBySpecialtyIdPaginatedAsync(specialtyId, 1, 10, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((new List<Doctor>(), 0));

        // Act
        var result = await _sut.Handle(
            new GetDoctorsBySpecialtyIdQuery(specialtyId, 1, 10),
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(0);

        _doctorRepositoryMock.Verify(
            x =>
                x.GetBySpecialtyIdPaginatedAsync(specialtyId, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
