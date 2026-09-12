using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.GuardianInvolvement;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.Services.Policies;
using ClinicFlow.Domain.Tests.Shared;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Domain.Tests.Services;

public class MedicalEncounterServiceTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IMedicalRecordValidationPolicy> _mockPolicy1;
    private readonly Mock<IMedicalRecordValidationPolicy> _mockPolicy2;
    private readonly Mock<IJsonSchemaValidator> _mockJsonValidator;
    private readonly MedicalEncounterService _sut;

    public MedicalEncounterServiceTests()
    {
        _mockPolicy1 = new Mock<IMedicalRecordValidationPolicy>();
        _mockPolicy2 = new Mock<IMedicalRecordValidationPolicy>();
        _mockJsonValidator = new Mock<IJsonSchemaValidator>();

        var policies = new List<IMedicalRecordValidationPolicy>
        {
            _mockPolicy1.Object,
            _mockPolicy2.Object,
        };
        _sut = new MedicalEncounterService(policies, _mockJsonValidator.Object);
    }

    [Fact]
    public void InitiateMedicalRecord_ShouldThrowArgumentNullException_WhenAppointmentIsNull()
    {
        //Arrange & Act
        var act = () => MedicalEncounterService.InitiateMedicalRecord(null!, "Chief complaint");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void InitiateMedicalRecord_ShouldThrowBusinessRuleValidationException_WhenAppointmentIsNotInProgress()
    {
        // Arrange
        var scheduledAppointment = Appointment.Schedule(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(10), new TimeOnly(11))
        );

        // Act
        var act = () =>
            MedicalEncounterService.InitiateMedicalRecord(scheduledAppointment, "Chief complaint");

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.MedicalEncounter.AppointmentNotInProgress);
    }

    [Fact]
    public void InitiateMedicalRecord_ShouldReturnMedicalRecord_WhenAppointmentIsInProgress()
    {
        // Arrange
        var appointment = CreateAppointment();
        var chiefComplaint = "Headache";

        // Act
        var result = MedicalEncounterService.InitiateMedicalRecord(appointment, chiefComplaint);

        // Assert
        result.Should().NotBeNull();
        result.AppointmentId.Should().Be(appointment.Id);
        result.PatientId.Should().Be(appointment.PatientId);
        result.DoctorId.Should().Be(appointment.DoctorId);
        result.ChiefComplaint.Should().Be(chiefComplaint);
    }

    [Fact]
    public void ValidateAndCompleteRecord_ShouldThrowArgumentNullException_WhenRecordIsNull()
    {
        // Act
        var act = () => _sut.ValidateAndCompleteRecord(null!, CreateValidContext());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValidateAndCompleteRecord_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange
        var record = CreateMedicalRecord();

        // Act
        var act = () => _sut.ValidateAndCompleteRecord(record, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValidateAndCompleteRecord_ShouldThrowArgumentNullException_WhenExpectedDoctorIsNull()
    {
        // Arrange
        var record = CreateMedicalRecord();

        // Act
        var act = () =>
            _sut.ValidateAndCompleteRecord(
                record,
                CreateValidContext() with
                {
                    ExpectedDoctor = null!,
                }
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValidateAndCompleteRecord_ShouldThrowArgumentNullException_WhenAppointmentIsNull()
    {
        // Arrange
        var record = CreateMedicalRecord();

        // Act
        var act = () =>
            _sut.ValidateAndCompleteRecord(
                record,
                CreateValidContext() with
                {
                    Appointment = null!,
                }
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValidateAndCompleteRecord_ShouldThrowArgumentNullException_WhenAppointmentTypeDefinitionIsNull()
    {
        // Arrange
        var record = CreateMedicalRecord();

        // Act
        var act = () =>
            _sut.ValidateAndCompleteRecord(
                record,
                CreateValidContext() with
                {
                    AppointmentTypeDefinition = null!,
                }
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValidateAndCompleteRecord_ShouldThrowBusinessRuleValidationException_WhenDoctorIdMismatch()
    {
        // Arrange
        var expectedDoctorId = Guid.CreateVersion7();
        var actualDoctorId = Guid.CreateVersion7();
        var appointmentId = Guid.CreateVersion7();
        var record = CreateMedicalRecord(actualDoctorId, appointmentId);
        var context = new MedicalEncounterContext
        {
            ExpectedDoctor = CreateDoctor(expectedDoctorId),
            Appointment = CreateAppointment(appointmentId),
            AppointmentTypeDefinition = CreateAppointmentType(),
            CompletedAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

        // Act
        var act = () => _sut.ValidateAndCompleteRecord(record, context);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.MedicalEncounter.DoctorMismatch);
    }

    [Fact]
    public void ValidateAndCompleteRecord_ShouldThrowBusinessRuleValidationException_WhenAppointmentIdMismatch()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var expectedAppointmentId = Guid.CreateVersion7();
        var actualAppointmentId = Guid.CreateVersion7();
        var record = CreateMedicalRecord(doctorId, actualAppointmentId);
        var context = new MedicalEncounterContext
        {
            ExpectedDoctor = CreateDoctor(doctorId),
            Appointment = CreateAppointment(expectedAppointmentId),
            AppointmentTypeDefinition = CreateAppointmentType(),
            CompletedAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

        // Act
        var act = () => _sut.ValidateAndCompleteRecord(record, context);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.MedicalEncounter.AppointmentMismatch);
    }

    [Fact]
    public void ValidateAndCompleteRecord_ShouldCallPoliciesAndAddDetails_WhenValid()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var appointmentId = Guid.CreateVersion7();
        var record = CreateMedicalRecord(doctorId, appointmentId);
        var appointmentType = CreateAppointmentType();
        var detail1 = DynamicClinicalDetail.Create("Test1", "{}");
        var detail2 = DynamicClinicalDetail.Create("Test2", "{}");
        var providedDetails = new List<DynamicClinicalDetail> { detail1, detail2 };

        var context = new MedicalEncounterContext
        {
            ExpectedDoctor = CreateDoctor(doctorId),
            Appointment = CreateAppointment(appointmentId),
            AppointmentTypeDefinition = appointmentType,
            CompletedAt = _fakeTime.GetUtcNow().UtcDateTime,
            ProvidedDetails = providedDetails,
        };

        // Act
        _sut.ValidateAndCompleteRecord(record, context);

        // Assert
        _mockPolicy1.Verify(p => p.Validate(appointmentType, providedDetails), Times.Once);
        _mockPolicy2.Verify(p => p.Validate(appointmentType, providedDetails), Times.Once);

        record.ClinicalDetails.Should().BeEquivalentTo([detail1, detail2]);
    }

    [Fact]
    public void ValidateAndCompleteRecord_ShouldCompleteAppointment_WhenValid()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var appointmentId = Guid.CreateVersion7();
        var record = CreateMedicalRecord(doctorId, appointmentId);
        var appointmentType = CreateAppointmentType();
        var detail = DynamicClinicalDetail.Create("Test1", "{}");
        var providedDetails = new List<DynamicClinicalDetail> { detail };

        var context = new MedicalEncounterContext
        {
            ExpectedDoctor = CreateDoctor(doctorId),
            Appointment = CreateAppointment(appointmentId),
            AppointmentTypeDefinition = appointmentType,
            CompletedAt = _fakeTime.GetUtcNow().UtcDateTime,
            ProvidedDetails = providedDetails,
        };

        // Act
        _sut.ValidateAndCompleteRecord(record, context);

        // Assert
        context.Appointment.Status.Should().Be(AppointmentStatus.Completed);
        context
            .Appointment.DomainEvents.OfType<AppointmentCompletedEvent>()
            .Should()
            .ContainSingle();
    }

    [Fact]
    public void AppendClinicalDetail_ShouldThrowArgumentNullException_WhenRecordIsNull()
    {
        // Arrange & Act
        var act = () =>
            _sut.AppendClinicalDetail(
                null!,
                DynamicClinicalDetail.Create("Test1", "{}"),
                CreateFormTemplate()
            );
        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AppendClinicalDetail_ShouldThrowArgumentNullException_WhenDetailIsNull()
    {
        // Arrange
        var record = CreateMedicalRecord();

        // Act
        var act = () => _sut.AppendClinicalDetail(record, null!, CreateFormTemplate());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AppendClinicalDetail_ShouldThrowArgumentNullException_WhenTemplateIsNull()
    {
        // Arrange
        var record = CreateMedicalRecord();

        // Act
        var act = () =>
            _sut.AppendClinicalDetail(record, DynamicClinicalDetail.Create("Test1", "{}"), null!);
        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AppendClinicalDetail_ShouldThrowBusinessRuleValidationException_WhenTemplateCodeMismatch()
    {
        // Arrange
        var record = CreateMedicalRecord();
        var detail = DynamicClinicalDetail.Create("Test1", "{}");
        var template = CreateFormTemplate("DifferentCode");

        // Act
        var act = () => _sut.AppendClinicalDetail(record, detail, template);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.MedicalEncounter.CodeMismatch);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AppendClinicalDetail_ShouldThrowBusinessRuleValidationException_WhenPayloadIsNullOrWhiteSpace(
        string? payload
    )
    {
        // Arrange
        var record = CreateMedicalRecord();
        var template = CreateFormTemplate("Test1");
        var detail = DynamicClinicalDetail.Create("Test1", payload!);

        // Act
        var act = () => _sut.AppendClinicalDetail(record, detail, template);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.MedicalEncounter.MissingPayload);
    }

    [Fact]
    public void AppendClinicalDetail_ShouldThrowBusinessRuleValidationException_WhenPayloadIsInvalidSchema()
    {
        // Arrange
        var record = CreateMedicalRecord();
        var detail = DynamicClinicalDetail.Create("Test1", """{"invalid": "data"}""");
        var template = CreateFormTemplate("Test1", """{"type": "object"}""");

        string errorMessage = "Schema validation failed";
        _mockJsonValidator
            .Setup(v =>
                v.ValidateSchema(
                    """{"type": "object"}""",
                    """{"invalid": "data"}""",
                    out errorMessage!
                )
            )
            .Returns(false);
        // Act
        var act = () => _sut.AppendClinicalDetail(record, detail, template);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage($"{DomainErrors.MedicalEncounter.ValidationFailed}: {errorMessage}");
    }

    [Fact]
    public void AppendClinicalDetail_ShouldAddDetail_WhenValidAndSchemaMatches()
    {
        // Arrange
        var record = CreateMedicalRecord();
        var detail = DynamicClinicalDetail.Create("Test1", """{"valid": "data"}""");
        var template = CreateFormTemplate("Test1", """{"type": "object"}""");

        string? errorMessage = null;
        _mockJsonValidator
            .Setup(v =>
                v.ValidateSchema(
                    """{"type": "object"}""",
                    """{"valid": "data"}""",
                    out errorMessage
                )
            )
            .Returns(true);
        // Act

        _sut.AppendClinicalDetail(record, detail, template);

        // Assert
        record.ClinicalDetails.Should().Contain(detail);
    }

    [Fact]
    public void AppendClinicalDetail_ShouldSkipSchemaValidation_WhenSchemaIsEmptyObject()
    {
        // Arrange
        var record = CreateMedicalRecord();
        var detail = DynamicClinicalDetail.Create("Test1", """{"bp":"120/80"}""");
        var template = CreateFormTemplate("Test1", "{}");

        string errorMessage = "Schema would fail if evaluated";
        _mockJsonValidator
            .Setup(v => v.ValidateSchema("{}", """{"bp":"120/80"}""", out errorMessage!))
            .Returns(false);

        // Act
        _sut.AppendClinicalDetail(record, detail, template);

        // Assert
        record.ClinicalDetails.Should().Contain(detail);
        _mockJsonValidator.Verify(
            v =>
                v.ValidateSchema(It.IsAny<string>(), It.IsAny<string>(), out It.Ref<string?>.IsAny),
            Times.Never
        );
    }

    [Fact]
    public void RecordGuardianInvolvementDetermination_ShouldThrowArgumentNullException_WhenRecordIsNull()
    {
        // Arrange & Act
        var act = () =>
            MedicalEncounterService.RecordGuardianInvolvementDetermination(
                null!,
                CreateAppointment(),
                new DoctorGuardianInvolvementArgs
                {
                    InitiatorDoctorId = Guid.CreateVersion7(),
                    GuardianInvolvementDeemedAppropriate = true,
                }
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RecordGuardianInvolvementDetermination_ShouldThrowArgumentNullException_WhenAppointmentIsNull()
    {
        // Arrange & Act
        var act = () =>
            MedicalEncounterService.RecordGuardianInvolvementDetermination(
                CreateMedicalRecord(),
                null!,
                new DoctorGuardianInvolvementArgs
                {
                    InitiatorDoctorId = Guid.CreateVersion7(),
                    GuardianInvolvementDeemedAppropriate = true,
                }
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RecordGuardianInvolvementDetermination_ShouldThrowArgumentNullException_WhenArgsIsNull()
    {
        // Arrange & Act
        var act = () =>
            MedicalEncounterService.RecordGuardianInvolvementDetermination(
                CreateMedicalRecord(),
                CreateAppointment(),
                null!
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RecordGuardianInvolvementDetermination_ShouldThrowBusinessRuleValidationException_WhenRecordBelongsToAnotherAppointment()
    {
        // Arrange
        var appointment = CreateAppointment();
        var record = MedicalRecord.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Headache",
            ProtectedCategory.MentalHealthCounseling,
            null
        );

        // Act
        var act = () =>
            MedicalEncounterService.RecordGuardianInvolvementDetermination(
                record,
                appointment,
                new DoctorGuardianInvolvementArgs
                {
                    InitiatorDoctorId = Guid.CreateVersion7(),
                    GuardianInvolvementDeemedAppropriate = true,
                }
            );

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.MedicalEncounter.AppointmentMismatch);
    }

    [Fact]
    public void RecordGuardianInvolvementDetermination_ShouldThrowDomainValidationException_WhenInitiatorIsNotAppointmentDoctor()
    {
        // Arrange
        var appointment = CreateAppointment();
        var record = CreateMedicalRecordWithCategoryForAppointment(
            ProtectedCategory.MentalHealthCounseling,
            appointment.Id
        );

        var args = new DoctorGuardianInvolvementArgs
        {
            InitiatorDoctorId = Guid.CreateVersion7(),
            GuardianInvolvementDeemedAppropriate = true,
        };

        // Act
        var act = () =>
            MedicalEncounterService.RecordGuardianInvolvementDetermination(
                record,
                appointment,
                args
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.UnauthorizedDoctor);
    }

    [Fact]
    public void RecordGuardianInvolvementDetermination_ShouldThrowBusinessRuleValidationException_WhenAppointmentIsNotInProgressOrCompleted()
    {
        // Arrange
        var scheduledAppointment = Appointment.Schedule(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(10), new TimeOnly(11))
        );
        var record = CreateMedicalRecordWithCategoryForAppointment(
            ProtectedCategory.MentalHealthCounseling,
            scheduledAppointment.Id
        );

        // Act
        var act = () =>
            MedicalEncounterService.RecordGuardianInvolvementDetermination(
                record,
                scheduledAppointment,
                new DoctorGuardianInvolvementArgs
                {
                    InitiatorDoctorId = scheduledAppointment.DoctorId,
                    GuardianInvolvementDeemedAppropriate = true,
                }
            );

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.MedicalEncounter.AppointmentNotInProgressOrCompleted);
    }

    [Fact]
    public void RecordGuardianInvolvementDetermination_ShouldSetDetermination_WhenAppointmentIsInProgress()
    {
        // Arrange
        var appointment = CreateAppointment();
        var record = CreateMedicalRecordWithCategoryForAppointment(
            ProtectedCategory.MentalHealthCounseling,
            appointment.Id
        );

        // Act
        MedicalEncounterService.RecordGuardianInvolvementDetermination(
            record,
            appointment,
            new DoctorGuardianInvolvementArgs
            {
                InitiatorDoctorId = appointment.DoctorId,
                GuardianInvolvementDeemedAppropriate = true,
            }
        );

        // Assert
        record.GuardianInvolvementDeemedAppropriate.Should().BeTrue();
    }

    [Fact]
    public void RecordGuardianInvolvementDetermination_ShouldSetDetermination_WhenAppointmentIsCompleted()
    {
        // Arrange
        var appointment = CreateAppointment();
        appointment.Complete(_fakeTime.GetUtcNow().UtcDateTime);
        var record = CreateMedicalRecordWithCategoryForAppointment(
            ProtectedCategory.ResidentialShelter,
            appointment.Id
        );

        // Act
        MedicalEncounterService.RecordGuardianInvolvementDetermination(
            record,
            appointment,
            new DoctorGuardianInvolvementArgs
            {
                InitiatorDoctorId = appointment.DoctorId,
                GuardianInvolvementDeemedAppropriate = false,
            }
        );

        // Assert
        record.GuardianInvolvementDeemedAppropriate.Should().BeFalse();
    }

    private static MedicalRecord CreateMedicalRecordWithCategoryForAppointment(
        ProtectedCategory protectedCareCategory,
        Guid appointmentId
    ) =>
        MedicalRecord.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            appointmentId,
            "Headache",
            protectedCareCategory,
            null
        );

    private MedicalEncounterContext CreateValidContext() =>
        new()
        {
            ExpectedDoctor = CreateDoctor(Guid.CreateVersion7()),
            Appointment = CreateAppointment(Guid.CreateVersion7()),
            AppointmentTypeDefinition = CreateAppointmentType(),
            CompletedAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

    private static MedicalRecord CreateMedicalRecord(Guid doctorId, Guid appointmentId) =>
        MedicalRecord.Create(
            Guid.CreateVersion7(),
            doctorId,
            appointmentId,
            "Headache",
            null,
            null
        );

    private static MedicalRecord CreateMedicalRecord() =>
        MedicalRecord.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Headache",
            null,
            null
        );

    private static Doctor CreateDoctor(Guid id)
    {
        var doctor = Doctor.Create(
            Guid.CreateVersion7(),
            PersonName.Create("Test Doctor"),
            MedicalLicenseNumber.Create("12345"),
            Guid.CreateVersion7(),
            "555-0000",
            ConsultationRoom.Create(1, "Room A", 1)
        );
        doctor.SetId(id);
        return doctor;
    }

    private Appointment CreateAppointment(Guid id)
    {
        var appointment = Appointment.Schedule(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(-1)),
            TimeRange.Create(new TimeOnly(10), new TimeOnly(11))
        );

        appointment.SetId(id);
        appointment.CheckIn(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime));
        appointment.Start(appointment.DoctorId, _fakeTime.GetUtcNow().UtcDateTime);

        return appointment;
    }

    private Appointment CreateAppointment()
    {
        var appointment = Appointment.Schedule(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(-1)),
            TimeRange.Create(new TimeOnly(10), new TimeOnly(11))
        );

        appointment.CheckIn(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime));
        appointment.Start(appointment.DoctorId, _fakeTime.GetUtcNow().UtcDateTime);

        return appointment;
    }

    private static AppointmentTypeDefinition CreateAppointmentType() =>
        AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Desc",
            EncounterDuration.FromMinutes(30)
        );

    private static ClinicalFormTemplate CreateFormTemplate(
        string code = "Test1",
        string jsonSchema = "{}"
    ) => ClinicalFormTemplate.Create(code, "Test Form", "Desc", jsonSchema);
}
