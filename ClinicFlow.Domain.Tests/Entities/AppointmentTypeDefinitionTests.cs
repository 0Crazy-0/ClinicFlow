using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Tests.Shared;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Tests.Entities;

public class AppointmentTypeDefinitionTests
{
    [Fact]
    public void Create_ShouldCreateInstance_WhenValidParameters()
    {
        // Arrange
        var category = AppointmentCategory.Checkup;
        var name = "General Checkup";
        var description = "Routine consultation";
        var duration = EncounterDuration.FromMinutes(30);

        // Act
        var result = AppointmentTypeDefinition.Create(category, name, description, duration);

        // Assert
        result.Should().NotBeNull();
        result.Category.Should().Be(category);
        result.Name.Should().Be(name);
        result.Description.Should().Be(description);
        result.Duration.Should().Be(duration);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenNameIsEmpty(string? name)
    {
        // Arrange & Act
        var act = () =>
            AppointmentTypeDefinition.Create(
                AppointmentCategory.Checkup,
                name!,
                "Description",
                EncounterDuration.FromMinutes(30)
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenDurationIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentTypeDefinition.Create(
                AppointmentCategory.Checkup,
                "Checkup",
                "Description",
                null!
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void AddRequiredTemplate_ShouldThrowException_WhenTemplateIsNull()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();

        // Act
        var act = () => appointmentType.AddRequiredTemplate(null!);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void AddRequiredTemplate_ShouldAddTemplate_WhenTemplateIsNew()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();
        var template = ClinicalFormTemplate.Create("CODE1", "Name", "Desc", "{}");

        // Act
        appointmentType.AddRequiredTemplate(template);

        // Assert
        appointmentType.RequiredTemplates.Should().ContainSingle().Which.Code.Should().Be("CODE1");
    }

    [Fact]
    public void AddRequiredTemplate_ShouldThrowException_WhenTemplateAlreadyExists_ById()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();
        var template1 = ClinicalFormTemplate.Create("CODE1", "Name", "Desc", "{}");
        var template2 = ClinicalFormTemplate.Create("CODE2", "Different", "Desc", "{}");
        var sharedId = Guid.NewGuid();

        template1.SetId(sharedId);
        template2.SetId(sharedId);

        appointmentType.AddRequiredTemplate(template1);

        // Act
        var act = () => appointmentType.AddRequiredTemplate(template2);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.TemplateAlreadyRequired);
    }

    [Fact]
    public void AddRequiredTemplate_ShouldThrowException_WhenTemplateAlreadyExists_ByCode()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();
        var template1 = ClinicalFormTemplate.Create("CODE1", "Name", "Desc", "{}");
        var template2 = ClinicalFormTemplate.Create("CODE1", "Another Name", "Desc", "{}");

        appointmentType.AddRequiredTemplate(template1);

        // Act
        var act = () => appointmentType.AddRequiredTemplate(template2);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.TemplateAlreadyRequired);
    }

    [Fact]
    public void RemoveRequiredTemplate_ShouldRemoveTemplate_WhenMatchingIdProvided()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();
        var templateToRemove = ClinicalFormTemplate.Create("CODE2", "Diff", "Desc", "{}");
        appointmentType.AddRequiredTemplate(templateToRemove);

        // Act
        appointmentType.RemoveRequiredTemplate(templateToRemove);

        // Assert
        appointmentType.RequiredTemplates.Should().BeEmpty();
    }

    [Fact]
    public void RemoveRequiredTemplate_ShouldThrowException_WhenTemplateIsNull()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();

        // Act
        var act = () => appointmentType.RemoveRequiredTemplate(null!);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void RemoveRequiredTemplate_ShouldThrow_WhenTemplateDoesNotExist()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();
        var template = ClinicalFormTemplate.Create("CODE1", "Name", "Desc", "{}");
        var unrelatedTemplate = ClinicalFormTemplate.Create("CODE2", "Other", "Desc", "{}");
        appointmentType.AddRequiredTemplate(template);

        // Act
        var act = () => appointmentType.RemoveRequiredTemplate(unrelatedTemplate);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.TemplateNotFound);
    }

    [Fact]
    public void ValidatePatientEligibility_ShouldNotThrowException_WhenPatientMeetsAllCriteria()
    {
        // Arrange
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Description",
            EncounterDuration.FromMinutes(30),
            AgeEligibilityPolicy.Create(18, 65, false)
        );

        // Act
        var act = () => appointmentType.ValidatePatientEligibility(30);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidatePatientEligibility_ShouldThrowException_WhenPatientIsTooYoung()
    {
        // Arrange
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Description",
            EncounterDuration.FromMinutes(30),
            AgeEligibilityPolicy.Create(18, null, false)
        );

        // Act
        var act = () => appointmentType.ValidatePatientEligibility(15);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.MinimumAgeNotMet);
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateAllProperties_WhenValidParameters()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();
        var newCategory = AppointmentCategory.FollowUp;
        var newName = "Updated Name";
        var newDescription = "Updated Description";
        var newDuration = EncounterDuration.FromMinutes(60);

        // Act
        appointmentType.UpdateDetails(newCategory, newName, newDescription, newDuration);

        // Assert
        appointmentType.Category.Should().Be(newCategory);
        appointmentType.Name.Should().Be(newName);
        appointmentType.Description.Should().Be(newDescription);
        appointmentType.Duration.Should().Be(newDuration);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDetails_ShouldThrowException_WhenNameIsEmpty(string? name)
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();

        // Act
        var act = () =>
            appointmentType.UpdateDetails(
                AppointmentCategory.Checkup,
                name!,
                "Description",
                EncounterDuration.FromMinutes(30)
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void UpdateDetails_ShouldThrowException_WhenDurationIsNull()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();

        // Act
        var act = () =>
            appointmentType.UpdateDetails(
                AppointmentCategory.Checkup,
                "Valid Name",
                "Description",
                null!
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void ChangeAgePolicy_ShouldUpdatePolicy_WhenValidPolicyProvided()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();
        var newPolicy = AgeEligibilityPolicy.Create(18, 65, false);

        // Act
        appointmentType.ChangeAgePolicy(newPolicy);

        // Assert
        appointmentType.AgePolicy.Should().Be(newPolicy);
    }

    [Fact]
    public void ChangeAgePolicy_ShouldSetNoRestriction_WhenNullProvided()
    {
        // Arrange
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Description",
            EncounterDuration.FromMinutes(30),
            AgeEligibilityPolicy.Create(18, 65, false)
        );

        // Act
        appointmentType.ChangeAgePolicy(null!);

        // Assert
        appointmentType.AgePolicy.Should().Be(AgeEligibilityPolicy.NoRestriction);
    }

    [Fact]
    public void MakeUnrestricted_ShouldSetUnrestrictedAndClearSpecialties_WhenCurrentlyRestricted()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();
        appointmentType.RestrictToSpecialties([Guid.NewGuid()]);

        // Act
        appointmentType.MakeUnrestricted();

        // Assert
        appointmentType.IsUnrestrictedBySpecialty.Should().BeTrue();
        appointmentType.AllowedSpecialtyIds.Should().BeEmpty();
    }

    [Fact]
    public void MakeUnrestricted_ShouldThrowException_WhenAlreadyUnrestricted()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();

        // Act & Assert
        appointmentType
            .Invoking(x => x.MakeUnrestricted())
            .Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.AlreadyUnrestricted);
    }

    [Fact]
    public void RestrictToSpecialties_ShouldSetSpecialties_WhenCurrentlyUnrestricted()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();
        var specialtyId1 = Guid.NewGuid();
        var specialtyId2 = Guid.NewGuid();

        // Act
        appointmentType.RestrictToSpecialties([specialtyId1, specialtyId2]);

        // Assert
        appointmentType.IsUnrestrictedBySpecialty.Should().BeFalse();
        appointmentType.AllowedSpecialtyIds.Should().HaveCount(2);
        appointmentType.AllowedSpecialtyIds.Should().Contain(specialtyId1);
        appointmentType.AllowedSpecialtyIds.Should().Contain(specialtyId2);
    }

    [Fact]
    public void RestrictToSpecialties_ShouldThrowException_WhenAlreadyRestricted()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();
        appointmentType.RestrictToSpecialties([Guid.NewGuid()]);

        // Act
        var act = () => appointmentType.RestrictToSpecialties([Guid.NewGuid()]);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.AlreadyRestricted);
    }

    [Fact]
    public void RestrictToSpecialties_ShouldThrowException_WhenListIsEmpty()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();

        // Act
        var act = () => appointmentType.RestrictToSpecialties([]);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.RequiresAtLeastOneSpecialty);
    }

    [Fact]
    public void RestrictToSpecialties_ShouldThrowException_WhenListIsNull()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();

        // Act
        var act = () => appointmentType.RestrictToSpecialties(null!);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.RequiresAtLeastOneSpecialty);
    }

    [Fact]
    public void RestrictToSpecialties_ShouldThrowException_WhenAnyIdIsEmpty()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();

        // Act
        var act = () => appointmentType.RestrictToSpecialties([Guid.NewGuid(), Guid.Empty]);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void RestrictToSpecialties_ShouldThrowException_WhenDuplicateIdsProvided()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();
        var duplicateId = Guid.NewGuid();

        // Act
        var act = () => appointmentType.RestrictToSpecialties([duplicateId, duplicateId]);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.DuplicateValues);
    }

    [Fact]
    public void AddAllowedSpecialty_ShouldAddSpecialty_WhenValidAndRestricted()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();
        var baseSpecialtyId = Guid.NewGuid();
        appointmentType.RestrictToSpecialties([baseSpecialtyId]);
        var specialtyId = Guid.NewGuid();

        // Act
        appointmentType.AddAllowedSpecialty(specialtyId);

        // Assert
        appointmentType.AllowedSpecialtyIds.Should().HaveCount(2);
        appointmentType.AllowedSpecialtyIds.Should().Contain(specialtyId);
    }

    [Fact]
    public void AddAllowedSpecialty_ShouldThrowException_WhenTypeIsUnrestricted()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();

        // Act
        var act = () => appointmentType.AddAllowedSpecialty(Guid.NewGuid());

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.CannotAddSpecialtyToGlobalType);
    }

    [Fact]
    public void AddAllowedSpecialty_ShouldThrowException_WhenSpecialtyIdIsEmpty()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();
        appointmentType.RestrictToSpecialties([Guid.NewGuid()]);

        // Act
        var act = () => appointmentType.AddAllowedSpecialty(Guid.Empty);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void AddAllowedSpecialty_ShouldThrowException_WhenSpecialtyAlreadyAllowed()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();
        var specialtyId = Guid.NewGuid();
        appointmentType.RestrictToSpecialties([specialtyId]);

        // Act
        var act = () => appointmentType.AddAllowedSpecialty(specialtyId);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.SpecialtyAlreadyAllowed);
    }

    [Fact]
    public void RemoveAllowedSpecialty_ShouldRemoveSpecialty_WhenItExists()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();
        var specialtyId1 = Guid.NewGuid();
        var specialtyId2 = Guid.NewGuid();
        appointmentType.RestrictToSpecialties([specialtyId1, specialtyId2]);

        // Act
        appointmentType.RemoveAllowedSpecialty(specialtyId1);

        // Assert
        appointmentType
            .AllowedSpecialtyIds.Should()
            .ContainSingle()
            .Which.Should()
            .Be(specialtyId2);
    }

    [Fact]
    public void RemoveAllowedSpecialty_ShouldThrowException_WhenTypeIsUnrestricted()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();

        // Act
        var act = () => appointmentType.RemoveAllowedSpecialty(Guid.NewGuid());

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.CannotRemoveSpecialtyFromGlobalType);
    }

    [Fact]
    public void RemoveAllowedSpecialty_ShouldThrowException_WhenSpecialtyDoesNotExist()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();
        var existingId = Guid.NewGuid();
        var existingId2 = Guid.NewGuid();
        appointmentType.RestrictToSpecialties([existingId, existingId2]);

        // Act
        var act = () => appointmentType.RemoveAllowedSpecialty(Guid.NewGuid());

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.SpecialtyNotFound);
    }

    [Fact]
    public void RemoveAllowedSpecialty_ShouldThrowException_WhenIsLastSpecialty()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();
        var specialtyId = Guid.NewGuid();
        appointmentType.RestrictToSpecialties([specialtyId]);

        // Act
        var act = () => appointmentType.RemoveAllowedSpecialty(specialtyId);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.RequiresAtLeastOneSpecialty);
    }

    [Fact]
    public void Deactivate_ShouldMarkAsDeleted_WhenActive()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();

        // Act
        appointmentType.Deactivate();

        // Assert
        appointmentType.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldThrowException_WhenAlreadyInactive()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();
        appointmentType.Deactivate();

        // Act & Assert
        appointmentType
            .Invoking(a => a.Deactivate())
            .Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.AppointmentType.AlreadyInactive);
    }

    [Fact]
    public void Reactivate_ShouldUnmarkAsDeleted_WhenInactive()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();
        appointmentType.Deactivate();

        // Act
        appointmentType.Reactivate();

        // Assert
        appointmentType.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Reactivate_ShouldThrowException_WhenAlreadyActive()
    {
        // Arrange
        var appointmentType = CreateAppointmentTypeDefinition();

        // Act & Assert
        appointmentType
            .Invoking(a => a.Reactivate())
            .Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.AppointmentType.AlreadyActive);
    }

    private static AppointmentTypeDefinition CreateAppointmentTypeDefinition() =>
        AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Description",
            EncounterDuration.FromMinutes(30)
        );
}
