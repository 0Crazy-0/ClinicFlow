using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Tests.Shared;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.Entities;

public class AppointmentTypeDefinitionTests
{
    [Fact]
    public void Create_ShouldCreateInstance_WhenValidParameters()
    {
        // Arrange
        var type = AppointmentCategory.Checkup;
        var name = "General Checkup";
        var description = "Routine consultation";
        var duration = TimeSpan.FromMinutes(30);

        // Act
        var result = new AppointmentTypeBuilder()
            .WithCategory(type)
            .WithName(name)
            .WithDescription(description)
            .WithDurationMinutes(duration)
            .Build();

        // Assert
        result.Should().NotBeNull();
        result.Category.Should().Be(type);
        result.Name.Should().Be(name);
        result.Description.Should().Be(description);
        result.DurationMinutes.Should().Be(duration);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenNameIsEmpty(string? name)
    {
        // Arrange & Act
        var act = () => new AppointmentTypeBuilder().WithName(name!).Build();

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [MemberData(nameof(InvalidDurations))]
    public void Create_ShouldThrowException_WhenDurationIsZeroOrNegative(TimeSpan duration)
    {
        // Arrange & Act
        var act = () => new AppointmentTypeBuilder().WithDurationMinutes(duration).Build();

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueMustBePositive);
    }

    [Fact]
    public void AddRequiredTemplate_ShouldThrowException_WhenTemplateIsNull()
    {
        // Arrange
        var appointmentType = new AppointmentTypeBuilder().Build();

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
        var appointmentType = new AppointmentTypeBuilder().Build();
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
        var appointmentType = new AppointmentTypeBuilder().Build();
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
        var appointmentType = new AppointmentTypeBuilder().Build();
        var template1 = ClinicalFormTemplate.Create("CODE1", "Name", "Desc", "{}");
        var template2 = ClinicalFormTemplate.Create("CODE1", "Another Name", "Desc", "{}");

        template1.SetId(Guid.NewGuid());
        template2.SetId(Guid.NewGuid());

        appointmentType.AddRequiredTemplate(template1);

        // Act
        var act = () => appointmentType.AddRequiredTemplate(template2);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.TemplateAlreadyRequired);
    }

    [Fact]
    public void RemoveRequiredTemplate_ShouldDoNothing_WhenTemplateIsNull()
    {
        // Arrange
        var appointmentType = new AppointmentTypeBuilder().Build();
        var template = ClinicalFormTemplate.Create("CODE1", "Name", "Desc", "{}");
        appointmentType.AddRequiredTemplate(template);

        // Act
        var act = () => appointmentType.RemoveRequiredTemplate(null!);

        // Assert
        act.Should().NotThrow();
        appointmentType.RequiredTemplates.Should().ContainSingle();
    }

    [Fact]
    public void RemoveRequiredTemplate_ShouldRemoveTemplate_WhenMatchingIdProvided()
    {
        // Arrange
        var appointmentType = new AppointmentTypeBuilder().Build();
        var template1 = ClinicalFormTemplate.Create("CODE1", "Name", "Desc", "{}");
        var templateToRemove = ClinicalFormTemplate.Create("CODE2", "Diff", "Desc", "{}");

        var sharedId = Guid.NewGuid();
        template1.SetId(sharedId);
        templateToRemove.SetId(sharedId);

        appointmentType.AddRequiredTemplate(template1);

        // Act
        appointmentType.RemoveRequiredTemplate(templateToRemove);

        // Assert
        appointmentType.RequiredTemplates.Should().BeEmpty();
    }

    [Fact]
    public void RemoveRequiredTemplate_ShouldRemoveTemplate_WhenMatchingCodeProvided()
    {
        // Arrange
        var appointmentType = new AppointmentTypeBuilder().Build();
        var template1 = ClinicalFormTemplate.Create("CODE1", "Name", "Desc", "{}");
        var templateToRemove = ClinicalFormTemplate.Create("CODE1", "Different", "Desc", "{}");

        templateToRemove.SetId(Guid.NewGuid());

        appointmentType.AddRequiredTemplate(template1);

        // Act
        appointmentType.RemoveRequiredTemplate(templateToRemove);

        // Assert
        appointmentType.RequiredTemplates.Should().BeEmpty();
    }

    [Fact]
    public void ValidatePatientEligibility_ShouldNotThrowException_WhenPatientMeetsAllCriteria()
    {
        // Arrange
        var appointmentType = new AppointmentTypeBuilder().WithAgePolicy(18, 65, false).Build();

        // Act
        var act = () => appointmentType.ValidatePatientEligibility(30);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidatePatientEligibility_ShouldThrowException_WhenPatientIsTooYoung()
    {
        // Arrange
        var appointmentType = new AppointmentTypeBuilder().WithAgePolicy(18, null, false).Build();

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
        var appointmentType = new AppointmentTypeBuilder().Build();
        var newCategory = AppointmentCategory.FollowUp;
        var newName = "Updated Name";
        var newDescription = "Updated Description";
        var newDuration = TimeSpan.FromMinutes(60);

        // Act
        appointmentType.UpdateDetails(newCategory, newName, newDescription, newDuration);

        // Assert
        appointmentType.Category.Should().Be(newCategory);
        appointmentType.Name.Should().Be(newName);
        appointmentType.Description.Should().Be(newDescription);
        appointmentType.DurationMinutes.Should().Be(newDuration);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDetails_ShouldThrowException_WhenNameIsEmpty(string? name)
    {
        // Arrange
        var appointmentType = new AppointmentTypeBuilder().Build();

        // Act
        var act = () =>
            appointmentType.UpdateDetails(
                AppointmentCategory.Checkup,
                name!,
                "Description",
                TimeSpan.FromMinutes(30)
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [MemberData(nameof(InvalidDurations))]
    public void UpdateDetails_ShouldThrowException_WhenDurationIsZeroOrNegative(TimeSpan duration)
    {
        // Arrange
        var appointmentType = new AppointmentTypeBuilder().Build();

        // Act
        var act = () =>
            appointmentType.UpdateDetails(
                AppointmentCategory.Checkup,
                "Valid Name",
                "Description",
                duration
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueMustBePositive);
    }

    [Fact]
    public void ChangeAgePolicy_ShouldUpdatePolicy_WhenValidPolicyProvided()
    {
        // Arrange
        var appointmentType = new AppointmentTypeBuilder().Build();
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
        var appointmentType = new AppointmentTypeBuilder().WithAgePolicy(18, 65, false).Build();

        // Act
        appointmentType.ChangeAgePolicy(null!);

        // Assert
        appointmentType.AgePolicy.Should().Be(AgeEligibilityPolicy.NoRestriction);
    }

    [Fact]
    public void MakeUnrestricted_ShouldSetUnrestrictedAndClearSpecialties_WhenCurrentlyRestricted()
    {
        // Arrange
        var appointmentType = new AppointmentTypeBuilder().Build();
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
        var appointmentType = new AppointmentTypeBuilder().Build();

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
        var appointmentType = new AppointmentTypeBuilder().Build();
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
        var appointmentType = new AppointmentTypeBuilder().Build();
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
        var appointmentType = new AppointmentTypeBuilder().Build();

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
        var appointmentType = new AppointmentTypeBuilder().Build();

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
        var appointmentType = new AppointmentTypeBuilder().Build();

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
        var appointmentType = new AppointmentTypeBuilder().Build();
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
        var appointmentType = new AppointmentTypeBuilder().Build();
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
        var appointmentType = new AppointmentTypeBuilder().Build();

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
        var appointmentType = new AppointmentTypeBuilder().Build();
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
        var appointmentType = new AppointmentTypeBuilder().Build();
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
        var appointmentType = new AppointmentTypeBuilder().Build();
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
        var appointmentType = new AppointmentTypeBuilder().Build();

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
        var appointmentType = new AppointmentTypeBuilder().Build();
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
        var appointmentType = new AppointmentTypeBuilder().Build();
        var specialtyId = Guid.NewGuid();
        appointmentType.RestrictToSpecialties([specialtyId]);

        // Act
        var act = () => appointmentType.RemoveAllowedSpecialty(specialtyId);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.RequiresAtLeastOneSpecialty);
    }

    private class AppointmentTypeBuilder
    {
        private AppointmentCategory _category = AppointmentCategory.Checkup;
        private string _name = "Checkup";
        private string _description = "Description";
        private TimeSpan _durationMinutes = TimeSpan.FromMinutes(30);
        private AgeEligibilityPolicy? _agePolicy = null;

        public AppointmentTypeBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public AppointmentTypeBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public AppointmentTypeBuilder WithDurationMinutes(TimeSpan duration)
        {
            _durationMinutes = duration;
            return this;
        }

        public AppointmentTypeBuilder WithAgePolicy(int? min, int? max, bool requiresGuardian)
        {
            _agePolicy = AgeEligibilityPolicy.Create(min, max, requiresGuardian);
            return this;
        }

        public AppointmentTypeBuilder WithCategory(AppointmentCategory category)
        {
            _category = category;
            return this;
        }

        public AppointmentTypeDefinition Build() =>
            AppointmentTypeDefinition.Create(
                _category,
                _name,
                _description,
                _durationMinutes,
                _agePolicy
            );
    }

    public static TheoryData<TimeSpan> InvalidDurations =>
        [TimeSpan.Zero, TimeSpan.FromMinutes(-10)];
}
