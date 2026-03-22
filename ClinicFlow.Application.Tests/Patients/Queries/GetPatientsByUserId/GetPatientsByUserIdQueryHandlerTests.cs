using ClinicFlow.Application.Patients.Queries.GetPatientsByUserId;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Patients.Queries.GetPatientsByUserId;

public class GetPatientsByUserIdQueryHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepositoryMock;
    private readonly GetPatientsByUserIdQueryHandler _sut;

    public GetPatientsByUserIdQueryHandlerTests()
    {
        _patientRepositoryMock = new Mock<IPatientRepository>();
        _sut = new GetPatientsByUserIdQueryHandler(_patientRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPatients_WhenPatientsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var patient1 = Patient.CreateSelf(
            userId,
            PersonName.Create("John Doe"),
            DateTime.UtcNow.AddYears(-30)
        );
        patient1.UpdateMedicalProfile(BloodType.Create("A+"), "None", "None");
        patient1.UpdateEmergencyContact(EmergencyContact.Create("Jane", "555-1234"));

        var patient2 = Patient.CreateFamilyMember(
            userId,
            PersonName.Create("Child Doe"),
            PatientRelationship.Child,
            DateTime.UtcNow.AddYears(-5)
        );
        patient2.UpdateMedicalProfile(BloodType.Create("A+"), "None", "None");
        patient2.UpdateEmergencyContact(EmergencyContact.Create("Jane", "555-1234"));

        var patients = new List<Patient> { patient1, patient2 };

        _patientRepositoryMock
            .Setup(x => x.GetAllByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patients);

        var query = new GetPatientsByUserIdQuery(userId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var resultList = result.ToList();
        resultList[0].FullName.Should().Be("John Doe");
        resultList[0].RelationshipToUser.Should().Be(PatientRelationship.Self);
        resultList[0].BloodType.Should().Be("A+");
        resultList[0].EmergencyContactName.Should().Be("Jane");
        resultList[0].EmergencyContactPhone.Should().Be("555-1234");

        resultList[1].FullName.Should().Be("Child Doe");
        resultList[1].RelationshipToUser.Should().Be(PatientRelationship.Child);
        resultList[1].BloodType.Should().Be("A+");
        resultList[1].EmergencyContactName.Should().Be("Jane");
        resultList[1].EmergencyContactPhone.Should().Be("555-1234");
    }

    [Fact]
    public async Task Handle_ShouldReturnPatients_WhenProfilesAreIncomplete()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var patient1 = Patient.CreateSelf(
            userId,
            PersonName.Create("John Doe"),
            DateTime.UtcNow.AddYears(-30)
        );
        var patient2 = Patient.CreateFamilyMember(
            userId,
            PersonName.Create("Child Doe"),
            PatientRelationship.Child,
            DateTime.UtcNow.AddYears(-5)
        );

        var patients = new List<Patient> { patient1, patient2 };

        _patientRepositoryMock
            .Setup(x => x.GetAllByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patients);

        var query = new GetPatientsByUserIdQuery(userId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var resultList = result.ToList();
        resultList[0].BloodType.Should().BeNull();
        resultList[0].EmergencyContactName.Should().BeNull();

        resultList[1].BloodType.Should().BeNull();
        resultList[1].EmergencyContactPhone.Should().BeNull();
    }
}
