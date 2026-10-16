using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.GuardianInvolvement;
using ClinicFlow.Domain.Services.Contexts.GuardianInvolvement;
using ClinicFlow.Domain.Services.Contexts.MedicalEncounter;
using ClinicFlow.Domain.Services.Policies;
using ClinicFlow.Domain.Tests.Shared;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Domain.Tests.Services;

public class MedicalEncounterServiceTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IJsonSchemaValidator> _mockJsonValidator;
    private readonly MedicalEncounterService _sut;

    public MedicalEncounterServiceTests()
    {
        _mockJsonValidator = new Mock<IJsonSchemaValidator>();

        var validationPolicy = new MetadataFormValidationPolicy(_mockJsonValidator.Object);
        _sut = new MedicalEncounterService(validationPolicy, _mockJsonValidator.Object);
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
            TimeRange.Create(new TimeOnly(10), new TimeOnly(11)),
            Guid.CreateVersion7()
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
                    TargetAppointment = null!,
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
                    AppointmentType = null!,
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
            TargetAppointment = CreateAppointment(appointmentId),
            AppointmentType = CreateAppointmentType(),
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
            TargetAppointment = CreateAppointment(expectedAppointmentId),
            AppointmentType = CreateAppointmentType(),
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
    public void ValidateAndCompleteRecord_ShouldValidateAgainstAccumulatedDetails_WhenValid()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var appointmentId = Guid.CreateVersion7();
        var record = CreateMedicalRecord(doctorId, appointmentId);
        var appointmentType = CreateAppointmentType();
        var detail1 = DynamicClinicalDetail.Create("Test1", "{}");
        var detail2 = DynamicClinicalDetail.Create("Test2", "{}");

        _sut.AppendClinicalDetail(record, detail1, CreateFormTemplate("Test1"));
        _sut.AppendClinicalDetail(record, detail2, CreateFormTemplate("Test2"));

        var context = new MedicalEncounterContext
        {
            ExpectedDoctor = CreateDoctor(doctorId),
            TargetAppointment = CreateAppointment(appointmentId),
            AppointmentType = appointmentType,
            CompletedAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

        // Act
        _sut.ValidateAndCompleteRecord(record, context);

        // Assert
        record.ClinicalDetails.Should().BeEquivalentTo([detail1, detail2]);
    }

    [Fact]
    public void ValidateAndCompleteRecord_ShouldThrowBusinessRuleValidationException_WhenRequiredTemplateIsMissing()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var appointmentId = Guid.CreateVersion7();
        var record = CreateMedicalRecord(doctorId, appointmentId);
        var appointmentType = CreateAppointmentType();

        appointmentType.AddRequiredTemplate(CreateFormTemplate("Test1", "{}"));

        var context = new MedicalEncounterContext
        {
            ExpectedDoctor = CreateDoctor(doctorId),
            TargetAppointment = CreateAppointment(appointmentId),
            AppointmentType = appointmentType,
            CompletedAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

        // Act
        var act = () => _sut.ValidateAndCompleteRecord(record, context);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.MedicalEncounter.MissingRequiredTemplate);
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

        _sut.AppendClinicalDetail(record, detail, CreateFormTemplate("Test1"));

        var context = new MedicalEncounterContext
        {
            ExpectedDoctor = CreateDoctor(doctorId),
            TargetAppointment = CreateAppointment(appointmentId),
            AppointmentType = appointmentType,
            CompletedAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

        // Act
        _sut.ValidateAndCompleteRecord(record, context);

        // Assert
        context.TargetAppointment.Status.Should().Be(AppointmentStatus.Completed);
        context
            .TargetAppointment.DomainEvents.OfType<AppointmentCompletedEvent>()
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

    [Fact]
    public void AppendClinicalDetail_ShouldAddDetail_WhenValidAndSchemaMatches()
    {
        // Arrange
        var record = CreateMedicalRecord();
        var detail = DynamicClinicalDetail.Create("Test1", """{"valid": "data"}""");
        var template = CreateFormTemplate("Test1", """{"type": "object"}""");

        // Act
        _sut.AppendClinicalDetail(record, detail, template);

        // Assert
        record.ClinicalDetails.Should().Contain(detail);
        _mockJsonValidator.Verify(
            v => v.ValidateSchema(template.JsonSchemaDefinition, detail.JsonDataPayload),
            Times.Once
        );
    }

    [Fact]
    public void AppendClinicalDetail_ShouldSkipSchemaValidation_WhenSchemaIsEmptyObject()
    {
        // Arrange
        var record = CreateMedicalRecord();
        var detail = DynamicClinicalDetail.Create("Test1", """{"bp":"120/80"}""");
        var template = CreateFormTemplate("Test1", "{}");

        // Act
        _sut.AppendClinicalDetail(record, detail, template);

        // Assert
        record.ClinicalDetails.Should().Contain(detail);
        _mockJsonValidator.Verify(
            v => v.ValidateSchema(It.IsAny<string>(), It.IsAny<string>()),
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
                CreateValidInvolvementArgs(),
                CreateValidInvolvementContext()
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
                CreateValidInvolvementArgs(),
                CreateValidInvolvementContext()
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
                null!,
                CreateValidInvolvementContext()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RecordGuardianInvolvementDetermination_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange & Act
        var act = () =>
            MedicalEncounterService.RecordGuardianInvolvementDetermination(
                CreateMedicalRecord(),
                CreateAppointment(),
                CreateValidInvolvementArgs(),
                null!
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RecordGuardianInvolvementDetermination_ShouldThrowArgumentNullException_WhenInitiatorDoctorIsNull()
    {
        // Arrange & Act
        var act = () =>
            MedicalEncounterService.RecordGuardianInvolvementDetermination(
                CreateMedicalRecord(),
                CreateAppointment(),
                CreateValidInvolvementArgs(),
                CreateValidInvolvementContext() with
                {
                    InitiatorDoctor = null!,
                }
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
                CreateValidInvolvementArgs(),
                CreateValidInvolvementContext()
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
            GuardianInvolvementDeemedAppropriate = true,
        };
        var context = new DoctorGuardianInvolvementContext
        {
            InitiatorDoctor = CreateDoctor(Guid.CreateVersion7()),
        };

        // Act
        var act = () =>
            MedicalEncounterService.RecordGuardianInvolvementDetermination(
                record,
                appointment,
                args,
                context
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
            TimeRange.Create(new TimeOnly(10), new TimeOnly(11)),
            Guid.CreateVersion7()
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
                new DoctorGuardianInvolvementArgs { GuardianInvolvementDeemedAppropriate = true },
                new DoctorGuardianInvolvementContext
                {
                    InitiatorDoctor = CreateDoctor(scheduledAppointment.DoctorId),
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
            new DoctorGuardianInvolvementArgs { GuardianInvolvementDeemedAppropriate = true },
            new DoctorGuardianInvolvementContext
            {
                InitiatorDoctor = CreateDoctor(appointment.DoctorId),
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
            new DoctorGuardianInvolvementArgs { GuardianInvolvementDeemedAppropriate = false },
            new DoctorGuardianInvolvementContext
            {
                InitiatorDoctor = CreateDoctor(appointment.DoctorId),
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
            TargetAppointment = CreateAppointment(Guid.CreateVersion7()),
            AppointmentType = CreateAppointmentType(),
            CompletedAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

    private static DoctorGuardianInvolvementArgs CreateValidInvolvementArgs() =>
        new() { GuardianInvolvementDeemedAppropriate = true };

    private static DoctorGuardianInvolvementContext CreateValidInvolvementContext() =>
        new() { InitiatorDoctor = CreateDoctor(Guid.CreateVersion7()) };

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
            TimeRange.Create(new TimeOnly(10), new TimeOnly(11)),
            Guid.CreateVersion7()
        );

        appointment.SetId(id);
        appointment.CheckIn(appointment.ScheduledDate.ToDateTime(appointment.TimeSlot.Start));
        appointment.Start(
            appointment.DoctorId,
            appointment.ScheduledDate.ToDateTime(new TimeOnly(10, 15))
        );

        return appointment;
    }

    private Appointment CreateAppointment()
    {
        var appointment = Appointment.Schedule(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(-1)),
            TimeRange.Create(new TimeOnly(10), new TimeOnly(11)),
            Guid.CreateVersion7()
        );

        appointment.CheckIn(appointment.ScheduledDate.ToDateTime(appointment.TimeSlot.Start));
        appointment.Start(
            appointment.DoctorId,
            appointment.ScheduledDate.ToDateTime(new TimeOnly(10, 15))
        );

        return appointment;
    }

    private static AppointmentTypeDefinition CreateAppointmentType() =>
        AppointmentTypeDefinition.Create(
            AppointmentCategory.Other,
            AppointmentPurpose.Checkup,
            "Checkup",
            "Desc",
            EncounterDuration.FromMinutes(30)
        );

    private static ClinicalFormTemplate CreateFormTemplate(
        string code = "Test1",
        string jsonSchema = "{}"
    ) => ClinicalFormTemplate.Create(code, "Test Form", "Desc", jsonSchema);
}
