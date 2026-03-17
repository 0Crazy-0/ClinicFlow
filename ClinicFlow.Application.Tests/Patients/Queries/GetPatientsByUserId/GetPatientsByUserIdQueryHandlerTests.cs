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
        var patients = new List<Patient>
        {
            Patient.CreateSelf(userId, PersonName.Create("John Doe"), DateTime.UtcNow.AddYears(-30), BloodType.Create("A+"), "None", "None",
            EmergencyContact.Create("Jane", "555-1234")),
            Patient.CreateFamilyMember(userId, PersonName.Create("Child Doe"), PatientRelationship.Child, DateTime.UtcNow.AddYears(-5), BloodType.Create("A+"), "None", "None",
             EmergencyContact.Create("Jane", "555-1234"))
        };

        _patientRepositoryMock.Setup(x => x.GetAllByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(patients);

        var query = new GetPatientsByUserIdQuery(userId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var resultList = result.ToList();
        resultList[0].FullName.Should().Be("John Doe");
        resultList[0].RelationshipToUser.Should().Be(PatientRelationship.Self);

        resultList[1].FullName.Should().Be("Child Doe");
        resultList[1].RelationshipToUser.Should().Be(PatientRelationship.Child);
    }
}
