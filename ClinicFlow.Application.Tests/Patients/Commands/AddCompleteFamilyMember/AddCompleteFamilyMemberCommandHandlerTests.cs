using AwesomeAssertions;
using ClinicFlow.Application.Patients.Commands.AddCompleteFamilyMember;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Patients.Commands.AddCompleteFamilyMember;

public class AddCompleteFamilyMemberCommandHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepositoryMock;
    private readonly Mock<IFamilyMembershipRepository> _familyMembershipRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FakeTimeProvider _fakeTime;
    private readonly AddCompleteFamilyMemberCommandHandler _sut;

    public AddCompleteFamilyMemberCommandHandlerTests()
    {
        _patientRepositoryMock = new Mock<IPatientRepository>();
        _familyMembershipRepositoryMock = new Mock<IFamilyMembershipRepository>();
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
            _familyMembershipRepositoryMock.Object,
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

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.HasActiveSelfMembershipByUserIdAsync(
                    command.UserId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        _patientRepositoryMock
            .Setup(x =>
                x.GetIncludingDeletedByNameAndDobAsync(
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

        FamilyMembership? capturedMembership = null;
        _familyMembershipRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<FamilyMembership>(), It.IsAny<CancellationToken>()))
            .Callback<FamilyMembership, CancellationToken>((m, _) => capturedMembership = m);

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeEmpty();
        capturedPatient.Should().NotBeNull();
        capturedPatient.FullName.ToString().Should().Be($"{command.FirstName} {command.LastName}");
        capturedPatient.DateOfBirth.Should().Be(command.DateOfBirth);
        capturedPatient.BloodType.ToString().Should().Be(command.BloodType);
        capturedPatient.Allergies.Should().Be(command.Allergies);
        capturedPatient.ChronicConditions.Should().Be(command.ChronicConditions);
        capturedPatient.EmergencyContact.Name.ToString().Should().Be(command.EmergencyContactName);
        capturedPatient
            .EmergencyContact.PhoneNumber.ToString()
            .Should()
            .Be(command.EmergencyContactPhone);

        capturedMembership.Should().NotBeNull();
        capturedMembership.PatientId.Should().Be(capturedPatient.Id);
        capturedMembership.UserId.Should().Be(command.UserId);
        capturedMembership.Role.Should().Be(command.Relationship);
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

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.HasActiveSelfMembershipByUserIdAsync(
                    command.UserId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        _patientRepositoryMock
            .Setup(x =>
                x.GetIncludingDeletedByNameAndDobAsync(
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
        _familyMembershipRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<FamilyMembership>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainValidationException_WhenDeletedProfileExists()
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

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.HasActiveSelfMembershipByUserIdAsync(
                    command.UserId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        var deletedMember = Patient.CreateProfile(
            personName,
            command.DateOfBirth,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        deletedMember.CloseAccount();

        _patientRepositoryMock
            .Setup(x =>
                x.GetIncludingDeletedByNameAndDobAsync(
                    personName,
                    command.DateOfBirth,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(deletedMember);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.ProfileRequiresAdministrativeClaim);

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
        _familyMembershipRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<FamilyMembership>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowPrimaryPatientRequiredException_WhenActiveSelfMembershipDoesNotExist()
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

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.HasActiveSelfMembershipByUserIdAsync(
                    command.UserId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<PrimaryPatientRequiredException>()
            .WithMessage(DomainErrors.Patient.PrimaryPatientRequired);
        exceptionAssertion.Which.UserId.Should().Be(command.UserId);

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
        _familyMembershipRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<FamilyMembership>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
