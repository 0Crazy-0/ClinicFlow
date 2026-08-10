using AwesomeAssertions;
using ClinicFlow.Application.Patients.Commands.CreatePatientProfile;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Patients.Commands.CreatePatientProfile;

public class CreatePatientProfileCommandHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepositoryMock;
    private readonly Mock<IFamilyMembershipRepository> _familyMembershipRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FakeTimeProvider _fakeTime;
    private readonly CreatePatientProfileCommandHandler _sut;

    public CreatePatientProfileCommandHandlerTests()
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

        _sut = new CreatePatientProfileCommandHandler(
            _fakeTime,
            _patientRepositoryMock.Object,
            _familyMembershipRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreatePatientProfile_WhenValidCommand()
    {
        // Arrange
        var command = new CreatePatientProfileCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30))
        );

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
        capturedPatient.BloodType.Should().BeNull();
        capturedPatient.EmergencyContact.Should().BeNull();

        capturedMembership.Should().NotBeNull();
        capturedMembership.PatientId.Should().Be(capturedPatient.Id);
        capturedMembership.UserId.Should().Be(command.UserId);
        capturedMembership.Role.Should().Be(PatientRelationship.Self);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryCreateAndSaveChanges_WhenValidCommand()
    {
        // Arrange
        var command = new CreatePatientProfileCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30))
        );

        _patientRepositoryMock
            .Setup(x =>
                x.GetIncludingDeletedByNameAndDobAsync(
                    It.IsAny<PersonName>(),
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
    public async Task Handle_ShouldThrowException_WhenDeletedProfileExists()
    {
        // Arrange
        var command = new CreatePatientProfileCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30))
        );

        var deletedPatient = Patient.CreateProfile(
            PersonName.Create($"{command.FirstName} {command.LastName}"),
            command.DateOfBirth,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        deletedPatient.CloseAccount();

        _patientRepositoryMock
            .Setup(x =>
                x.GetIncludingDeletedByNameAndDobAsync(
                    It.IsAny<PersonName>(),
                    command.DateOfBirth,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(deletedPatient);

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
}
