using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services;

public class PatientAccessServiceTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void EnsureCanActOnBehalfOf_ShouldThrowDomainValidationException_WhenInitiatorIsNull()
    {
        // Arrange
        var target = CreateSelfPatient(Guid.CreateVersion7());

        // Act
        var act = () => PatientAccessService.EnsureCanActOnBehalfOf(null!, target);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void EnsureCanActOnBehalfOf_ShouldThrowDomainValidationException_WhenTargetIsNull()
    {
        // Arrange
        var initiator = CreateSelfPatient(Guid.CreateVersion7());

        // Act
        var act = () => PatientAccessService.EnsureCanActOnBehalfOf(initiator, null!);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void EnsureCanActOnBehalfOf_ShouldNotThrow_WhenInitiatorIsSelf()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var initiator = CreateSelfPatient(userId);
        var target = initiator;

        // Act
        var act = () => PatientAccessService.EnsureCanActOnBehalfOf(initiator, target);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCanActOnBehalfOf_ShouldNotThrow_WhenInitiatorIsParent()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var initiator = CreateSelfPatient(userId);
        var target = CreateFamilyMember(userId, PatientRelationship.Child, 10);

        // Act
        var act = () => PatientAccessService.EnsureCanActOnBehalfOf(initiator, target);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCanActOnBehalfOf_ShouldThrowPatientAccessUnauthorizedException_WhenUserIdsDoNotMatch()
    {
        // Arrange
        var initiator = CreateSelfPatient(Guid.CreateVersion7());
        var target = CreateSelfPatient(Guid.CreateVersion7());

        // Act
        var act = () => PatientAccessService.EnsureCanActOnBehalfOf(initiator, target);

        // Assert
        act.Should()
            .Throw<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);
    }

    [Fact]
    public void EnsureCanActOnBehalfOf_ShouldThrowPatientAccessUnauthorizedException_WhenInitiatorIsNotSelfOrChild()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var initiator = CreateFamilyMember(userId, PatientRelationship.Spouse, 30);
        var target = CreateFamilyMember(userId, PatientRelationship.Child, 10);

        // Act
        var act = () => PatientAccessService.EnsureCanActOnBehalfOf(initiator, target);

        // Assert
        act.Should()
            .Throw<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);
    }

    private Patient CreateSelfPatient(Guid userId) =>
        Patient.CreateSelf(
            userId,
            PersonName.Create("Test"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

    private Patient CreateFamilyMember(Guid userId, PatientRelationship relationship, int age) =>
        Patient.CreateFamilyMember(
            userId,
            PersonName.Create("Family"),
            relationship,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-age)),
            _fakeTime.GetUtcNow().UtcDateTime
        );
}
