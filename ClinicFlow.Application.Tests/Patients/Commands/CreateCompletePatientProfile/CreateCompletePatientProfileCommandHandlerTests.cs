using ClinicFlow.Application.Patients.Commands.CreateCompletePatientProfile;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Patients.Commands.CreateCompletePatientProfile;

public class CreateCompletePatientProfileCommandHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateCompletePatientProfileCommandHandler _sut;

    public CreateCompletePatientProfileCommandHandlerTests()
    {
        _patientRepositoryMock = new Mock<IPatientRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new CreateCompletePatientProfileCommandHandler(_patientRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateCompletePatientProfile_WhenValidCommand()
    {
        // Arrange
        var command = new CreateCompletePatientProfileCommand(Guid.NewGuid(), "John", "Doe", DateTime.UtcNow.AddYears(-30), "O+", "None", "None", "Mom", "555-5555");

        Patient? capturedPatient = null;
        _patientRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()))
            .Callback<Patient, CancellationToken>((p, _) => capturedPatient = p).ReturnsAsync((Patient p, CancellationToken _) => p);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        capturedPatient.Should().NotBeNull();
        capturedPatient!.UserId.Should().Be(command.UserId);
        capturedPatient.FullName.ToString().Should().Be($"{command.FirstName} {command.LastName}");
        capturedPatient.RelationshipToUser.Should().Be(PatientRelationship.Self);
        capturedPatient.DateOfBirth.Should().Be(command.DateOfBirth);
        capturedPatient.BloodType.ToString().Should().Be(command.BloodType);
        capturedPatient.Allergies.Should().Be(command.Allergies);
        capturedPatient.ChronicConditions.Should().Be(command.ChronicConditions);
        capturedPatient.EmergencyContact.Name.ToString().Should().Be(command.EmergencyContactName);
        capturedPatient.EmergencyContact.PhoneNumber.ToString().Should().Be(command.EmergencyContactPhone);

        capturedPatient.HasCompleteMedicalProfile().Should().BeTrue();
    }
}
