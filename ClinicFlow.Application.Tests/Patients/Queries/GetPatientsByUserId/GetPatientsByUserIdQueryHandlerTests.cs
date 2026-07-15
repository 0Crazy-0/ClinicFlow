using AwesomeAssertions;
using ClinicFlow.Application.Patients.Queries.DTOs;
using ClinicFlow.Application.Patients.Queries.GetPatientsByUserId;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Patients.Queries.GetPatientsByUserId;

public class GetPatientsByUserIdQueryHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepositoryMock;
    private readonly FakeTimeProvider _fakeTime;
    private readonly GetPatientsByUserIdQueryHandler _sut;

    public GetPatientsByUserIdQueryHandlerTests()
    {
        _patientRepositoryMock = new Mock<IPatientRepository>();
        _fakeTime = new FakeTimeProvider();
        _sut = new GetPatientsByUserIdQueryHandler(_patientRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPatients_WhenPatientsExist()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var patient1 = Patient.CreateSelf(
            userId,
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient1.UpdateMedicalProfile(BloodType.Create("A+"), "None", "None");
        patient1.UpdateEmergencyContact(EmergencyContact.Create("Jane", "555-1234"));

        var patient2 = Patient.CreateFamilyMember(
            userId,
            PersonName.Create("Child Doe"),
            PatientRelationship.Child,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-5)),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient2.UpdateMedicalProfile(BloodType.Create("A+"), "None", "None");
        patient2.UpdateEmergencyContact(EmergencyContact.Create("Jane", "555-1234"));

        var patients = new List<Patient> { patient1, patient2 };

        _patientRepositoryMock
            .Setup(x => x.GetAllByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patients);

        var query = new GetPatientsByUserIdQuery(userId);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDtos = patients.Select(patient => new PatientDto(
            patient.Id,
            patient.UserId,
            patient.FullName.FullName,
            patient.RelationshipToUser,
            patient.DateOfBirth,
            patient.BloodType?.Value,
            patient.Allergies,
            patient.ChronicConditions,
            patient.EmergencyContact?.Name.ToString(),
            patient.EmergencyContact?.PhoneNumber.ToString()
        ));

        result.Should().BeEquivalentTo(expectedDtos);

        _patientRepositoryMock.Verify(
            x => x.GetAllByUserIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnPatients_WhenProfilesAreIncomplete()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var patient1 = Patient.CreateSelf(
            userId,
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var patient2 = Patient.CreateFamilyMember(
            userId,
            PersonName.Create("Child Doe"),
            PatientRelationship.Child,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-5)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var patients = new List<Patient> { patient1, patient2 };

        _patientRepositoryMock
            .Setup(x => x.GetAllByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patients);

        var query = new GetPatientsByUserIdQuery(userId);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDtos = patients.Select(patient => new PatientDto(
            patient.Id,
            patient.UserId,
            patient.FullName.FullName,
            patient.RelationshipToUser,
            patient.DateOfBirth,
            patient.BloodType?.Value,
            patient.Allergies,
            patient.ChronicConditions,
            patient.EmergencyContact?.Name.ToString(),
            patient.EmergencyContact?.PhoneNumber.ToString()
        ));

        result.Should().BeEquivalentTo(expectedDtos);

        _patientRepositoryMock.Verify(
            x => x.GetAllByUserIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
