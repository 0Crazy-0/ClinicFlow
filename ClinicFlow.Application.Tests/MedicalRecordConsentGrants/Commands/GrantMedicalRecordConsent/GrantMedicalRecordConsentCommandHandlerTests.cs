using AwesomeAssertions;
using ClinicFlow.Application.Common.Utilities;
using ClinicFlow.Application.MedicalRecordConsentGrants.Commands.GrantMedicalRecordConsent;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.MedicalRecordConsentGrants.Commands.GrantMedicalRecordConsent;

public class GrantMedicalRecordConsentCommandHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IFamilyMembershipRepository> _familyMembershipRepositoryMock = new();
    private readonly Mock<IMedicalRecordRepository> _medicalRecordRepositoryMock = new();
    private readonly Mock<IMedicalRecordConsentGrantRepository> _consentGrantRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly GrantMedicalRecordConsentCommandHandler _sut;

    public GrantMedicalRecordConsentCommandHandlerTests()
    {
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

        _sut = new GrantMedicalRecordConsentCommandHandler(
            _fakeTime,
            _patientRepositoryMock.Object,
            _familyMembershipRepositoryMock.Object,
            _medicalRecordRepositoryMock.Object,
            _consentGrantRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateGrant_WhenValidCommand()
    {
        // Arrange
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var requesterMembership = FamilyMembership.CreateSelf(
            patient.Id,
            requesterUserId,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var recipientMembership = CreateRecipientMembership(patient.Id);
        var record = CreateProtectedRecord(patient.Id);
        var command = new GrantMedicalRecordConsentCommand(
            requesterUserId,
            patient.Id,
            record.Id,
            recipientMembership.Id
        );

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patient.Id,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(requesterMembership);

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        _familyMembershipRepositoryMock
            .Setup(x => x.GetByIdAsync(recipientMembership.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipientMembership);

        _consentGrantRepositoryMock
            .Setup(x =>
                x.HasEffectiveGrantAsync(
                    record.Id,
                    recipientMembership.Id,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);

        MedicalRecordConsentGrant? capturedGrant = null;
        _consentGrantRepositoryMock
            .Setup(x =>
                x.CreateAsync(It.IsAny<MedicalRecordConsentGrant>(), It.IsAny<CancellationToken>())
            )
            .Callback<MedicalRecordConsentGrant, CancellationToken>((g, _) => capturedGrant = g);

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeEmpty();
        capturedGrant.Should().NotBeNull();
        capturedGrant.PatientId.Should().Be(patient.Id);
        capturedGrant.RecipientMembershipId.Should().Be(recipientMembership.Id);
        capturedGrant.MedicalRecordId.Should().Be(record.Id);
        capturedGrant.SignedByUserId.Should().Be(requesterUserId);
        capturedGrant.SignedAt.Should().Be(_fakeTime.GetLocalNow().DateTime);
        capturedGrant
            .ExpiresAt.Should()
            .Be(capturedGrant.SignedAt.AddYears(MedicalRecordConsentGrant.ValidityYears));
        capturedGrant.RevokedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryCreateAndSaveChanges_WhenValidCommand()
    {
        // Arrange
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var requesterMembership = FamilyMembership.CreateSelf(
            patient.Id,
            requesterUserId,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var recipientMembership = CreateRecipientMembership(patient.Id);
        var record = CreateProtectedRecord(patient.Id);
        var command = new GrantMedicalRecordConsentCommand(
            requesterUserId,
            patient.Id,
            record.Id,
            recipientMembership.Id
        );

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patient.Id,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(requesterMembership);

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        _familyMembershipRepositoryMock
            .Setup(x => x.GetByIdAsync(recipientMembership.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipientMembership);

        _consentGrantRepositoryMock
            .Setup(x =>
                x.HasEffectiveGrantAsync(
                    record.Id,
                    recipientMembership.Id,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var expectedLockKey = DeterministicKeyGenerator.FromComposite(
            command.MedicalRecordId.ToString(),
            command.RecipientMembershipId.ToString()
        );

        _unitOfWorkMock.Verify(
            x =>
                x.ExecuteWithLockAsync(
                    expectedLockKey,
                    It.IsAny<Func<CancellationToken, Task<Guid>>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );

        _consentGrantRepositoryMock.Verify(
            x =>
                x.CreateAsync(It.IsAny<MedicalRecordConsentGrant>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenPatientDoesNotExist()
    {
        // Arrange
        var command = new GrantMedicalRecordConsentCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7()
        );

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(command.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var act = () => _sut.Handle(command, TestContext.Current.CancellationToken);

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
        _consentGrantRepositoryMock.Verify(
            x =>
                x.CreateAsync(It.IsAny<MedicalRecordConsentGrant>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenRequesterMembershipDoesNotExist()
    {
        // Arrange
        var patient = CreateMinorPatient();
        var command = new GrantMedicalRecordConsentCommand(
            Guid.CreateVersion7(),
            patient.Id,
            Guid.CreateVersion7(),
            Guid.CreateVersion7()
        );

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    command.RequesterUserId,
                    command.PatientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((FamilyMembership?)null);

        // Act
        var act = () => _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(FamilyMembership));

        _unitOfWorkMock.Verify(
            x =>
                x.ExecuteWithLockAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Func<CancellationToken, Task<Guid>>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _consentGrantRepositoryMock.Verify(
            x =>
                x.CreateAsync(It.IsAny<MedicalRecordConsentGrant>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenMedicalRecordDoesNotExist()
    {
        // Arrange
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var requesterMembership = FamilyMembership.CreateSelf(
            patient.Id,
            requesterUserId,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var command = new GrantMedicalRecordConsentCommand(
            requesterUserId,
            patient.Id,
            Guid.CreateVersion7(),
            Guid.CreateVersion7()
        );

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patient.Id,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(requesterMembership);
        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(command.MedicalRecordId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MedicalRecord?)null);

        // Act
        var act = () => _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(MedicalRecord));

        _unitOfWorkMock.Verify(
            x =>
                x.ExecuteWithLockAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Func<CancellationToken, Task<Guid>>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _consentGrantRepositoryMock.Verify(
            x =>
                x.CreateAsync(It.IsAny<MedicalRecordConsentGrant>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenRecipientMembershipDoesNotExist()
    {
        // Arrange
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var requesterMembership = FamilyMembership.CreateSelf(
            patient.Id,
            requesterUserId,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var record = CreateProtectedRecord(patient.Id);
        var command = new GrantMedicalRecordConsentCommand(
            requesterUserId,
            patient.Id,
            record.Id,
            Guid.CreateVersion7()
        );

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patient.Id,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(requesterMembership);

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetByIdAsync(command.RecipientMembershipId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((FamilyMembership?)null);

        // Act
        var act = () => _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(FamilyMembership));

        _unitOfWorkMock.Verify(
            x =>
                x.ExecuteWithLockAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Func<CancellationToken, Task<Guid>>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _consentGrantRepositoryMock.Verify(
            x =>
                x.CreateAsync(It.IsAny<MedicalRecordConsentGrant>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private Patient CreateMinorPatient() =>
        Patient.CreateProfile(
            PersonName.Create("Jane Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime).AddYears(-15),
            _fakeTime.GetUtcNow().UtcDateTime
        );

    private FamilyMembership CreateRecipientMembership(Guid patientId) =>
        FamilyMembership.CreateFamilyMember(
            patientId,
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

    private static MedicalRecord CreateProtectedRecord(Guid patientId) =>
        MedicalRecord.Create(
            patientId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "General",
            ProtectedCategory.MentalHealthCounseling,
            null
        );
}
