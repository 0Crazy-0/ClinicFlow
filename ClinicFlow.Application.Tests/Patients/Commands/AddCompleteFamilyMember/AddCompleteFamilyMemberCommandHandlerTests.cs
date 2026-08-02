using AwesomeAssertions;
using ClinicFlow.Application.Patients.Commands.AddCompleteFamilyMember;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
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

        _unitOfWorkMock
            .Setup(x =>
                x.ExecuteWithLockAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Func<CancellationToken, Task<Guid>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(
                (
                    Guid _,
                    Func<CancellationToken, Task<Guid>> operation,
                    CancellationToken cancellationToken
                ) => operation(cancellationToken)
            );

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
            Guid.CreateVersion7(),
            "Child",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-5)),
            "A+",
            "Peanuts",
            "Asthma",
            "Mom",
            "555-5555",
            PatientRelationship.Child
        );

        _patientRepositoryMock
            .Setup(x =>
                x.GetIncludingDeletedByNameAndDobAsync(
                    command.UserId,
                    It.IsAny<PersonName>(),
                    command.DateOfBirth,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((Patient?)null);

        Patient? capturedPatient = null;
        _patientRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()))
            .Callback<Patient, CancellationToken>((p, _) => capturedPatient = p);

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeEmpty();
        capturedPatient.Should().NotBeNull();
        capturedPatient.UserId.Should().Be(command.UserId);
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
    public async Task Handle_ShouldReactivateFamilyMemberAndUpdateMedicalProfile_WhenDeletedProfileExists()
    {
        // Arrange
        var command = new AddCompleteFamilyMemberCommand(
            Guid.CreateVersion7(),
            "Child",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-5)),
            "O+",
            "Pollen",
            "None",
            "Dad",
            "555-9999",
            PatientRelationship.Child
        );
        var personName = PersonName.Create($"{command.FirstName} {command.LastName}");

        var deletedMember = Patient.CreateFamilyMember(
            command.UserId,
            personName,
            PatientRelationship.Sibling,
            command.DateOfBirth,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        deletedMember.RemoveFamilyMember(command.UserId);

        _patientRepositoryMock
            .Setup(x =>
                x.GetIncludingDeletedByNameAndDobAsync(
                    command.UserId,
                    personName,
                    command.DateOfBirth,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(deletedMember);

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _unitOfWorkMock.Verify(
            x =>
                x.ExecuteWithLockAsync(
                    command.UserId,
                    It.IsAny<Func<CancellationToken, Task<Guid>>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _patientRepositoryMock.Verify(
            r => r.CreateAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        result.Should().Be(deletedMember.Id);
        deletedMember.IsDeleted.Should().BeFalse();
        deletedMember.RelationshipToUser.Should().Be(PatientRelationship.Child);
        deletedMember.BloodType.ToString().Should().Be(command.BloodType);
        deletedMember.Allergies.Should().Be(command.Allergies);
        deletedMember.ChronicConditions.Should().Be(command.ChronicConditions);
        deletedMember.EmergencyContact.Name.ToString().Should().Be(command.EmergencyContactName);
        deletedMember
            .EmergencyContact.PhoneNumber.ToString()
            .Should()
            .Be(command.EmergencyContactPhone);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryCreateAndSaveChanges_WhenValidCommand()
    {
        // Arrange
        var command = new AddCompleteFamilyMemberCommand(
            Guid.CreateVersion7(),
            "Child",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-5)),
            "A+",
            "None",
            "None",
            "Mom",
            "555-5555",
            PatientRelationship.Child
        );
        var personName = PersonName.Create($"{command.FirstName} {command.LastName}");

        _patientRepositoryMock
            .Setup(x =>
                x.GetIncludingDeletedByNameAndDobAsync(
                    command.UserId,
                    personName,
                    command.DateOfBirth,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((Patient?)null);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _unitOfWorkMock.Verify(
            x =>
                x.ExecuteWithLockAsync(
                    command.UserId,
                    It.IsAny<Func<CancellationToken, Task<Guid>>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _patientRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRelationshipIsSelf()
    {
        // Arrange
        var command = new AddCompleteFamilyMemberCommand(
            Guid.CreateVersion7(),
            "Self",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            "A+",
            "None",
            "None",
            "Mom",
            "555-5555",
            PatientRelationship.Self
        );

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<DomainValidationException>();
        _unitOfWorkMock.Verify(
            x =>
                x.ExecuteWithLockAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Func<CancellationToken, Task<Guid>>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _patientRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
