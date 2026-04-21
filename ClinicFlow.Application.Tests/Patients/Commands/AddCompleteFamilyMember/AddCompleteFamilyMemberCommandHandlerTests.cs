using ClinicFlow.Application.Patients.Commands.AddCompleteFamilyMember;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Patients.Commands.AddCompleteFamilyMember;

public class AddCompleteFamilyMemberCommandHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FakeTimeProvider _fakeTime;
    private readonly AddCompleteFamilyMemberCommandHandler _sut;

    public AddCompleteFamilyMemberCommandHandlerTests()
    {
        _patientRepositoryMock = new Mock<IPatientRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fakeTime = new FakeTimeProvider();
        _sut = new AddCompleteFamilyMemberCommandHandler(
            _fakeTime,
            _patientRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateFamilyMember_WhenValidCommand()
    {
        // Arrange
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "Child",
            "Doe",
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-5).Date,
            "A+",
            "None",
            "None",
            "Mom",
            "555-5555",
            PatientRelationship.Child
        );

        Patient? capturedPatient = null;
        _patientRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()))
            .Callback<Patient, CancellationToken>((p, _) => capturedPatient = p)
            .ReturnsAsync((Patient p, CancellationToken _) => p);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        capturedPatient.Should().NotBeNull();
        capturedPatient!.UserId.Should().Be(command.UserId);
        capturedPatient.FullName.ToString().Should().Be($"{command.FirstName} {command.LastName}");
        capturedPatient.RelationshipToUser.Should().Be(command.Relationship);
        capturedPatient.DateOfBirth.Should().Be(command.DateOfBirth);
        capturedPatient.BloodType.ToString().Should().Be(command.BloodType);
        capturedPatient.Allergies.Should().Be(command.Allergies);
        capturedPatient.ChronicConditions.Should().Be(command.ChronicConditions);
        capturedPatient.EmergencyContact.Name.ToString().Should().Be(command.EmergencyContactName);
        capturedPatient
            .EmergencyContact.PhoneNumber.ToString()
            .Should()
            .Be(command.EmergencyContactPhone);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRelationshipIsSelf()
    {
        // Arrange
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "Self",
            "Doe",
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30).Date,
            "A+",
            "None",
            "None",
            "Mom",
            "555-5555",
            PatientRelationship.Self
        );

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainValidationException>();
        _patientRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
