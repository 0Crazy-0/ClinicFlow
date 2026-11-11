using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.GuardianNotes;
using ClinicFlow.Domain.Services.Contexts.GuardianNotes;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services;

public class AppointmentGuardianNotesServiceTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void UpdateByGuardian_ShouldThrowArgumentNullException_WhenAppointmentIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentGuardianNotesService.UpdateByGuardian(
                null!,
                CreateValidArgs(),
                CreateValidContext()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UpdateByGuardian_ShouldThrowArgumentNullException_WhenArgsIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentGuardianNotesService.UpdateByGuardian(
                CreateAppointment(Guid.CreateVersion7()),
                null!,
                CreateValidContext()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UpdateByGuardian_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentGuardianNotesService.UpdateByGuardian(
                CreateAppointment(Guid.CreateVersion7()),
                CreateValidArgs(),
                null!
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UpdateByGuardian_ShouldThrowArgumentNullException_WhenTargetPatientIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentGuardianNotesService.UpdateByGuardian(
                CreateAppointment(Guid.CreateVersion7()),
                CreateValidArgs(),
                CreateValidContext() with
                {
                    TargetPatient = null!,
                }
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UpdateByGuardian_ShouldThrowArgumentNullException_WhenInitiatorMembershipIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentGuardianNotesService.UpdateByGuardian(
                CreateAppointment(Guid.CreateVersion7()),
                CreateValidArgs(),
                CreateValidContext() with
                {
                    InitiatorMembership = null!,
                }
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UpdateByGuardian_ShouldThrowValidation_WhenTargetMismatch()
    {
        // Arrange
        var appointment = CreateAppointment(Guid.CreateVersion7()); // patientId is different from target patient
        var target = CreateMinorPatient();
        var authorId = Guid.CreateVersion7();
        var args = new UpdateGuardianNotesArgs { Notes = "Note" };
        var context = new UpdateGuardianNotesContext
        {
            TargetPatient = target,
            InitiatorMembership = CreateParentMembership(target.Id, authorId),
        };

        // Act
        var act = () =>
            AppointmentGuardianNotesService.UpdateByGuardian(appointment, args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.DataMismatch);
    }

    [Fact]
    public void UpdateByGuardian_ShouldThrowValidation_WhenMembershipMismatch()
    {
        // Arrange
        var target = CreateMinorPatient();
        var appointment = CreateAppointment(target.Id);
        var authorId = Guid.CreateVersion7();
        var args = new UpdateGuardianNotesArgs { Notes = "Note" };
        var context = new UpdateGuardianNotesContext
        {
            TargetPatient = target,
            InitiatorMembership = CreateParentMembership(Guid.CreateVersion7(), authorId),
        };

        // Act
        var act = () =>
            AppointmentGuardianNotesService.UpdateByGuardian(appointment, args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.DataMismatch);
    }

    [Fact]
    public void UpdateByGuardian_ShouldUpdateGuardianNotes_WhenAuthorGuardianUpdates()
    {
        // Arrange
        var target = CreateMinorPatient();
        var authorId = Guid.CreateVersion7();
        var appointment = CreateAppointment(target.Id);

        appointment.UpdateGuardianNotes("Original", authorId);

        var args = new UpdateGuardianNotesArgs { Notes = "Updated note" };
        var context = new UpdateGuardianNotesContext
        {
            TargetPatient = target,
            InitiatorMembership = CreateParentMembership(target.Id, authorId),
        };

        // Act
        AppointmentGuardianNotesService.UpdateByGuardian(appointment, args, context);

        // Assert
        appointment.GuardianNotes.Should().Be(args.Notes);
        appointment.GuardianNotesAuthorUserId.Should().Be(authorId);
        appointment.PatientNotes.Should().BeEmpty();
    }

    [Fact]
    public void UpdateByGuardian_ShouldUpdateGuardianNotes_WhenGuardianCreatesFirstNote()
    {
        // Arrange
        var target = CreateMinorPatient();
        var authorId = Guid.CreateVersion7();
        var appointment = CreateAppointment(target.Id);

        var args = new UpdateGuardianNotesArgs { Notes = "First note" };
        var context = new UpdateGuardianNotesContext
        {
            TargetPatient = target,
            InitiatorMembership = CreateParentMembership(target.Id, authorId),
        };

        // Act
        AppointmentGuardianNotesService.UpdateByGuardian(appointment, args, context);

        // Assert
        appointment.GuardianNotes.Should().Be(args.Notes);
        appointment.GuardianNotesAuthorUserId.Should().Be(authorId);
        appointment.PatientNotes.Should().BeEmpty();
    }

    [Fact]
    public void UpdateByGuardian_ShouldThrowUnauthorized_WhenNonAuthorGuardianUpdates()
    {
        // Arrange
        var target = CreateMinorPatient();
        var authorId = Guid.CreateVersion7();
        var otherGuardianId = Guid.CreateVersion7();

        var appointment = CreateAppointment(target.Id);
        appointment.UpdateGuardianNotes("Original", authorId);

        var args = new UpdateGuardianNotesArgs { Notes = "Outsider attempt" };
        var context = new UpdateGuardianNotesContext
        {
            TargetPatient = target,
            InitiatorMembership = CreateParentMembership(target.Id, otherGuardianId),
        };

        // Act
        var act = () =>
            AppointmentGuardianNotesService.UpdateByGuardian(appointment, args, context);

        // Assert
        act.Should()
            .Throw<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);

        appointment.GuardianNotes.Should().Be("Original");
    }

    [Fact]
    public void UpdateByGuardian_ShouldThrowUnauthorized_WhenInitiatorIsSelf()
    {
        // Arrange
        var target = CreateMinorPatient();
        var userId = Guid.CreateVersion7();
        var appointment = CreateAppointment(target.Id);

        var args = new UpdateGuardianNotesArgs { Notes = "Note" };
        var context = new UpdateGuardianNotesContext
        {
            TargetPatient = target,
            InitiatorMembership = FamilyMembership.CreateSelf(
                target.Id,
                userId,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
        };

        // Act
        var act = () =>
            AppointmentGuardianNotesService.UpdateByGuardian(appointment, args, context);

        // Assert
        act.Should()
            .Throw<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);
    }

    [Fact]
    public void UpdateByGuardian_ShouldThrowUnauthorized_WhenAuthorityIsNone()
    {
        // Arrange
        var target = CreateMinorPatient();
        var userId = Guid.CreateVersion7();
        var appointment = CreateAppointment(target.Id);

        var args = new UpdateGuardianNotesArgs { Notes = "Note" };
        var context = new UpdateGuardianNotesContext
        {
            TargetPatient = target,
            InitiatorMembership = FamilyMembership.CreateAdultFamilyMember(
                target.Id,
                userId,
                PatientRelationship.Spouse,
                FamilyMembershipAccessLevel.Full,
                FamilyMembership.MinimumAdultAge,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
        };

        // Act
        var act = () =>
            AppointmentGuardianNotesService.UpdateByGuardian(appointment, args, context);

        // Assert
        act.Should()
            .Throw<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);
    }

    [Fact]
    public void UpdateByGuardian_ShouldThrowValidation_WhenPatientIsAdult()
    {
        // Arrange
        var patientAdult = Patient.CreateProfile(
            PersonName.Create("Adult", null, "Adult"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-25)),
            BiologicalSex.Male,
            Address.Create(
                "123 Main Street",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var authorId = Guid.CreateVersion7();
        var appointment = CreateAppointment(patientAdult.Id);

        var args = new UpdateGuardianNotesArgs { Notes = "Note for adult" };
        var context = new UpdateGuardianNotesContext
        {
            TargetPatient = patientAdult,
            InitiatorMembership = CreateParentMembership(patientAdult.Id, authorId),
        };

        // Act
        var act = () =>
            AppointmentGuardianNotesService.UpdateByGuardian(appointment, args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.GuardianRequiresMinor);

        appointment.GuardianNotes.Should().BeEmpty();
        appointment.GuardianNotesAuthorUserId.Should().BeNull();
    }

    [Fact]
    public void UpdateByGuardian_ShouldThrowValidation_WhenPatientIsExactlyAdultAge()
    {
        // Arrange
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2));
        var target = Patient.CreateProfile(
            PersonName.Create("Child", null, "Child"),
            scheduledDate.AddYears(-FamilyMembership.MinimumAdultAge),
            BiologicalSex.Male,
            Address.Create(
                "123 Main Street",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        target.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        target.UpdateEmergencyContact(EmergencyContact.Create("Name", null, "Name", "1234567890"));

        var authorId = Guid.CreateVersion7();
        var appointment = CreateAppointment(target.Id);

        var args = new UpdateGuardianNotesArgs { Notes = "Note" };
        var context = new UpdateGuardianNotesContext
        {
            TargetPatient = target,
            InitiatorMembership = CreateParentMembership(target.Id, authorId),
        };

        // Act
        var act = () =>
            AppointmentGuardianNotesService.UpdateByGuardian(appointment, args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.GuardianRequiresMinor);

        appointment.GuardianNotes.Should().BeEmpty();
        appointment.GuardianNotesAuthorUserId.Should().BeNull();
    }

    [Fact]
    public void UpdateByGuardian_ShouldThrowValidation_WhenStatusIsNotAllowed()
    {
        // Arrange
        var target = CreateMinorPatient();
        var authorId = Guid.CreateVersion7();
        var appointment = CreateAppointment(target.Id);
        appointment.MarkAsRequiresReassignment();

        var args = new UpdateGuardianNotesArgs { Notes = "Note" };
        var context = new UpdateGuardianNotesContext
        {
            TargetPatient = target,
            InitiatorMembership = CreateParentMembership(target.Id, authorId),
        };

        // Act
        var act = () =>
            AppointmentGuardianNotesService.UpdateByGuardian(appointment, args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.CannotUpdateNotes);
    }

    [Fact]
    public void ClearByGuardian_ShouldThrowArgumentNullException_WhenAppointmentIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentGuardianNotesService.ClearByGuardian(null!, CreateValidContext());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ClearByGuardian_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentGuardianNotesService.ClearByGuardian(
                CreateAppointment(Guid.CreateVersion7()),
                null!
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ClearByGuardian_ShouldThrowArgumentNullException_WhenTargetPatientIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentGuardianNotesService.ClearByGuardian(
                CreateAppointment(Guid.CreateVersion7()),
                CreateValidContext() with
                {
                    TargetPatient = null!,
                }
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ClearByGuardian_ShouldThrowArgumentNullException_WhenInitiatorMembershipIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentGuardianNotesService.ClearByGuardian(
                CreateAppointment(Guid.CreateVersion7()),
                CreateValidContext() with
                {
                    InitiatorMembership = null!,
                }
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ClearByGuardian_ShouldThrowValidation_WhenTargetMismatch()
    {
        // Arrange
        var target = CreateMinorPatient();
        var appointment = CreateAppointment(Guid.CreateVersion7()); // different patient id
        var context = new UpdateGuardianNotesContext
        {
            TargetPatient = target,
            InitiatorMembership = CreateParentMembership(target.Id, Guid.CreateVersion7()),
        };

        // Act
        var act = () => AppointmentGuardianNotesService.ClearByGuardian(appointment, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.DataMismatch);
    }

    [Fact]
    public void ClearByGuardian_ShouldThrowValidation_WhenMembershipMismatch()
    {
        // Arrange
        var target = CreateMinorPatient();
        var appointment = CreateAppointment(target.Id);
        var context = new UpdateGuardianNotesContext
        {
            TargetPatient = target,
            InitiatorMembership = CreateParentMembership(
                Guid.CreateVersion7(),
                Guid.CreateVersion7()
            ),
        };

        // Act
        var act = () => AppointmentGuardianNotesService.ClearByGuardian(appointment, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.DataMismatch);
    }

    [Fact]
    public void ClearByGuardian_ShouldThrowValidation_WhenNoteIsEmptyAndStatusIsNotAllowed()
    {
        // Arrange
        var target = CreateMinorPatient();
        var appointment = CreateAppointment(target.Id);

        appointment.MarkAsRequiresReassignment();

        var context = new UpdateGuardianNotesContext
        {
            TargetPatient = target,
            InitiatorMembership = CreateParentMembership(target.Id, Guid.CreateVersion7()),
        };

        // Act
        var act = () => AppointmentGuardianNotesService.ClearByGuardian(appointment, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.CannotClearNotes);
    }

    [Fact]
    public void ClearByGuardian_ShouldClearNotesAndAuthor_WhenOriginalAuthorClears()
    {
        // Arrange
        var target = CreateMinorPatient();
        var authorId = Guid.CreateVersion7();
        var appointment = CreateAppointment(target.Id);

        appointment.UpdateGuardianNotes("note", authorId);

        var context = new UpdateGuardianNotesContext
        {
            TargetPatient = target,
            InitiatorMembership = CreateParentMembership(target.Id, authorId),
        };

        // Act
        AppointmentGuardianNotesService.ClearByGuardian(appointment, context);

        // Assert
        appointment.GuardianNotes.Should().BeEmpty();
        appointment.GuardianNotesAuthorUserId.Should().BeNull();
    }

    [Fact]
    public void ClearByGuardian_ShouldThrowUnauthorized_WhenNonAuthorGuardianClears()
    {
        // Arrange
        var target = CreateMinorPatient();
        var appointment = CreateAppointment(target.Id);

        appointment.UpdateGuardianNotes("notes", Guid.CreateVersion7());

        var context = new UpdateGuardianNotesContext
        {
            TargetPatient = target,
            InitiatorMembership = CreateParentMembership(target.Id, Guid.CreateVersion7()),
        };

        // Act
        var act = () => AppointmentGuardianNotesService.ClearByGuardian(appointment, context);

        // Assert
        act.Should()
            .Throw<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);

        appointment.GuardianNotes.Should().Be("notes");
    }

    [Fact]
    public void ClearByGuardian_ShouldThrowUnauthorized_WhenInitiatorIsSelf()
    {
        // Arrange
        var target = CreateMinorPatient();
        var appointment = CreateAppointment(target.Id);

        appointment.UpdateGuardianNotes("Original", Guid.CreateVersion7());

        var context = new UpdateGuardianNotesContext
        {
            TargetPatient = target,
            InitiatorMembership = FamilyMembership.CreateSelf(
                target.Id,
                Guid.CreateVersion7(),
                _fakeTime.GetUtcNow().UtcDateTime
            ),
        };

        // Act
        var act = () => AppointmentGuardianNotesService.ClearByGuardian(appointment, context);

        // Assert
        act.Should()
            .Throw<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);
    }

    [Fact]
    public void ClearByGuardian_ShouldThrowUnauthorized_WhenAuthorMembershipLostFullAccess()
    {
        // Arrange
        var target = CreateMinorPatient();
        var authorId = Guid.CreateVersion7();
        var appointment = CreateAppointment(target.Id);
        var membership = CreateParentMembership(target.Id, authorId);

        appointment.UpdateGuardianNotes("notes", authorId);

        membership.ChangeAccessLevel(
            FamilyMembershipAccessLevel.ViewOnly,
            FamilyMembership.MinimumAdultAge,
            requesterIsAuthorized: true
        );

        var context = new UpdateGuardianNotesContext
        {
            TargetPatient = target,
            InitiatorMembership = membership,
        };

        // Act
        var act = () => AppointmentGuardianNotesService.ClearByGuardian(appointment, context);

        // Assert
        act.Should()
            .Throw<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);

        appointment.GuardianNotes.Should().Be("notes");
        appointment.GuardianNotesAuthorUserId.Should().Be(authorId);
    }

    [Fact]
    public void ClearByGuardian_ShouldThrowValidation_WhenPatientIsAdult()
    {
        // Arrange
        var adultPatient = Patient.CreateProfile(
            PersonName.Create("rainbow", null, "dash"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-21)),
            BiologicalSex.Female,
            Address.Create(
                "123 Main Street",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var appointment = CreateAppointment(adultPatient.Id);
        var authorId = Guid.CreateVersion7();

        appointment.UpdateGuardianNotes("notes", authorId);

        var context = new UpdateGuardianNotesContext
        {
            TargetPatient = adultPatient,
            InitiatorMembership = CreateParentMembership(adultPatient.Id, authorId),
        };

        // Act
        var act = () => AppointmentGuardianNotesService.ClearByGuardian(appointment, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.GuardianRequiresMinor);
    }

    [Fact]
    public void ClearByGuardian_ShouldThrowValidation_WhenPatientIsExactlyAdultAge()
    {
        // Arrange
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2));
        var target = Patient.CreateProfile(
            PersonName.Create("discord", null, "test"),
            scheduledDate.AddYears(-FamilyMembership.MinimumAdultAge),
            BiologicalSex.Male,
            Address.Create(
                "123 Main Street",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var authorId = Guid.CreateVersion7();
        var appointment = CreateAppointment(target.Id);
        appointment.UpdateGuardianNotes("notes", authorId);

        var context = new UpdateGuardianNotesContext
        {
            TargetPatient = target,
            InitiatorMembership = CreateParentMembership(target.Id, authorId),
        };

        // Act
        var act = () => AppointmentGuardianNotesService.ClearByGuardian(appointment, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.GuardianRequiresMinor);

        appointment.GuardianNotes.Should().Be("notes");
        appointment.GuardianNotesAuthorUserId.Should().Be(authorId);
    }

    private FamilyMembership CreateParentMembership(Guid patientId, Guid userId) =>
        FamilyMembership.CreateMinorFamilyMember(
            patientId,
            userId,
            PatientRelationship.Parent,
            LegalAuthorityType.Parent,
            FamilyMembership.MinimumAdultAge - 1,
            _fakeTime.GetUtcNow().UtcDateTime
        );

    private Patient CreateMinorPatient()
    {
        var patient = Patient.CreateProfile(
            PersonName.Create("Child", null, "Child"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-10)),
            BiologicalSex.Male,
            Address.Create(
                "123 Main Street",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Name", null, "Name", "1234567890"));

        return patient;
    }

    private static UpdateGuardianNotesArgs CreateValidArgs() => new() { Notes = "Note" };

    private UpdateGuardianNotesContext CreateValidContext()
    {
        var target = CreateMinorPatient();

        return new()
        {
            TargetPatient = target,
            InitiatorMembership = CreateParentMembership(target.Id, Guid.CreateVersion7()),
        };
    }

    private Appointment CreateAppointment(Guid patientId)
    {
        var appointment = Appointment.Schedule(
            patientId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)),
            Guid.CreateVersion7()
        );

        appointment.ClearDomainEvents();

        return appointment;
    }
}
