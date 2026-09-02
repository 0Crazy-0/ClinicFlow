using AwesomeAssertions;
using ClinicFlow.Application.FamilyMemberships.Commands.AddCompleteFamilyMember;
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

namespace ClinicFlow.Application.Tests.FamilyMemberships.Commands.AddCompleteFamilyMember;

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
            PatientRelationship.Child,
            FamilyMembershipAccessLevel.Full
        );

        var ownerPatient = Patient.CreateProfile(
            PersonName.Create("Parent Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-35)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var ownerMembership = FamilyMembership.CreateSelf(
            ownerPatient.Id,
            command.UserId,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveSelfMembershipByUserIdAsync(
                    command.UserId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(ownerMembership);

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(ownerPatient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownerPatient);

        _patientRepositoryMock
            .Setup(x =>
                x.GetByNameAndDobAsync(
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
            PatientRelationship.Child,
            FamilyMembershipAccessLevel.Full
        );
        var personName = PersonName.Create($"{command.FirstName} {command.LastName}");

        var ownerPatient = Patient.CreateProfile(
            PersonName.Create("Parent Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-35)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var ownerMembership = FamilyMembership.CreateSelf(
            ownerPatient.Id,
            command.UserId,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveSelfMembershipByUserIdAsync(
                    command.UserId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(ownerMembership);

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(ownerPatient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownerPatient);

        _patientRepositoryMock
            .Setup(x =>
                x.GetByNameAndDobAsync(
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
            PatientRelationship.Child,
            FamilyMembershipAccessLevel.Full
        );

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveSelfMembershipByUserIdAsync(
                    command.UserId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((FamilyMembership?)null);

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

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenOwnerPatientDoesNotExist()
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
            PatientRelationship.Child,
            FamilyMembershipAccessLevel.Full
        );

        var ownerPatientId = Guid.CreateVersion7();

        var ownerMembership = FamilyMembership.CreateSelf(
            ownerPatientId,
            command.UserId,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveSelfMembershipByUserIdAsync(
                    command.UserId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(ownerMembership);

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(ownerPatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Patient));

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
