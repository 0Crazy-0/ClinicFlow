using AwesomeAssertions;
using ClinicFlow.Application.AppointmentConsentGrants.Commands.GrantSpecificAppointmentConsent;
using ClinicFlow.Application.Common.Utilities;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.AppointmentConsentGrants.Commands.GrantSpecificAppointmentConsent;

public class GrantSpecificAppointmentConsentCommandHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IFamilyMembershipRepository> _familyMembershipRepositoryMock = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly Mock<IAppointmentTypeDefinitionRepository> _appointmentTypeRepositoryMock =
        new();
    private readonly Mock<IAppointmentConsentGrantRepository> _consentGrantRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly GrantSpecificAppointmentConsentCommandHandler _sut;

    public GrantSpecificAppointmentConsentCommandHandlerTests()
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

        _sut = new GrantSpecificAppointmentConsentCommandHandler(
            _fakeTime,
            _patientRepositoryMock.Object,
            _familyMembershipRepositoryMock.Object,
            _appointmentRepositoryMock.Object,
            _appointmentTypeRepositoryMock.Object,
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
        var appointmentType = CreateProtectedAppointmentType();
        var appointment = CreateAppointment(patient.Id, appointmentType.Id);
        var command = new GrantSpecificAppointmentConsentCommand(
            requesterUserId,
            patient.Id,
            recipientMembership.Id,
            appointment.Id
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

        _familyMembershipRepositoryMock
            .Setup(x => x.GetByIdAsync(recipientMembership.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipientMembership);

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _appointmentTypeRepositoryMock
            .Setup(x =>
                x.GetByIdAsync(appointment.AppointmentTypeId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(appointmentType);

        _consentGrantRepositoryMock
            .Setup(x =>
                x.HasEffectiveAppointmentGrantAsync(
                    patient.Id,
                    recipientMembership.Id,
                    appointment.Id,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);

        AppointmentConsentGrant? capturedGrant = null;
        _consentGrantRepositoryMock
            .Setup(x =>
                x.CreateAsync(It.IsAny<AppointmentConsentGrant>(), It.IsAny<CancellationToken>())
            )
            .Callback<AppointmentConsentGrant, CancellationToken>((g, _) => capturedGrant = g);

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeEmpty();
        capturedGrant.Should().NotBeNull();
        capturedGrant.PatientId.Should().Be(patient.Id);
        capturedGrant.RecipientMembershipId.Should().Be(recipientMembership.Id);
        capturedGrant.Scope.Should().Be(AppointmentConsentScope.SpecificAppointment);
        capturedGrant.AppointmentId.Should().Be(appointment.Id);
        capturedGrant.ProtectedCareCategory.Should().BeNull();
        capturedGrant.SignedByUserId.Should().Be(requesterUserId);
        capturedGrant.SignedAt.Should().Be(_fakeTime.GetLocalNow().DateTime);
        capturedGrant
            .ExpiresAt.Should()
            .Be(capturedGrant.SignedAt.AddYears(AppointmentConsentGrant.ValidityYears));
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
        var appointmentType = CreateProtectedAppointmentType();
        var appointment = CreateAppointment(patient.Id, appointmentType.Id);
        var command = new GrantSpecificAppointmentConsentCommand(
            requesterUserId,
            patient.Id,
            recipientMembership.Id,
            appointment.Id
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

        _familyMembershipRepositoryMock
            .Setup(x => x.GetByIdAsync(recipientMembership.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipientMembership);

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _appointmentTypeRepositoryMock
            .Setup(x =>
                x.GetByIdAsync(appointment.AppointmentTypeId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(appointmentType);

        _consentGrantRepositoryMock
            .Setup(x =>
                x.HasEffectiveAppointmentGrantAsync(
                    patient.Id,
                    recipientMembership.Id,
                    appointment.Id,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var expectedLockKey = DeterministicKeyGenerator.FromComposite(
            command.PatientId.ToString(),
            $"{command.RecipientMembershipId}{command.AppointmentId}"
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
            x => x.CreateAsync(It.IsAny<AppointmentConsentGrant>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenPatientDoesNotExist()
    {
        // Arrange
        var command = new GrantSpecificAppointmentConsentCommand(
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
            x => x.CreateAsync(It.IsAny<AppointmentConsentGrant>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenRequesterMembershipDoesNotExist()
    {
        // Arrange
        var patient = CreateMinorPatient();
        var command = new GrantSpecificAppointmentConsentCommand(
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
            x => x.CreateAsync(It.IsAny<AppointmentConsentGrant>(), It.IsAny<CancellationToken>()),
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
        var command = new GrantSpecificAppointmentConsentCommand(
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
            x => x.CreateAsync(It.IsAny<AppointmentConsentGrant>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentDoesNotExist()
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
        var command = new GrantSpecificAppointmentConsentCommand(
            requesterUserId,
            patient.Id,
            recipientMembership.Id,
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

        _familyMembershipRepositoryMock
            .Setup(x => x.GetByIdAsync(recipientMembership.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipientMembership);

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        // Act
        var act = () => _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Appointment));

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
            x => x.CreateAsync(It.IsAny<AppointmentConsentGrant>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentTypeDoesNotExist()
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
        var appointmentType = CreateProtectedAppointmentType();
        var appointment = CreateAppointment(patient.Id, appointmentType.Id);
        var command = new GrantSpecificAppointmentConsentCommand(
            requesterUserId,
            patient.Id,
            recipientMembership.Id,
            appointment.Id
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

        _familyMembershipRepositoryMock
            .Setup(x => x.GetByIdAsync(recipientMembership.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipientMembership);

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _appointmentTypeRepositoryMock
            .Setup(x =>
                x.GetByIdAsync(appointment.AppointmentTypeId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((AppointmentTypeDefinition?)null);

        // Act
        var act = () => _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(AppointmentTypeDefinition));

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
            x => x.CreateAsync(It.IsAny<AppointmentConsentGrant>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private Patient CreateMinorPatient() =>
        Patient.CreateProfile(
            PersonName.Create("Jane Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime).AddYears(-15),
            BiologicalSex.Male,
            Address.Create(
                "123 Main Street",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
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

    private static AppointmentTypeDefinition CreateProtectedAppointmentType() =>
        AppointmentTypeDefinition.Create(
            AppointmentCategory.Other,
            AppointmentPurpose.Checkup,
            "Checkup",
            "Description",
            EncounterDuration.FromMinutes(30),
            null,
            ProtectedCategory.MentalHealthCounseling
        );

    private Appointment CreateAppointment(Guid patientId, Guid appointmentTypeId) =>
        Appointment.Schedule(
            patientId,
            Guid.CreateVersion7(),
            appointmentTypeId,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(9), new TimeOnly(10)),
            Guid.CreateVersion7()
        );
}
