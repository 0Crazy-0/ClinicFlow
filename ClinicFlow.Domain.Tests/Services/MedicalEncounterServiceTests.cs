using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services;
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
    public void InitiateMedicalRecord_ShouldThrowDomainValidationException_WhenAppointmentIsNull()
    {
        // Act
        var act = () => MedicalEncounterService.InitiateMedicalRecord(null!, "Chief complaint");

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void InitiateMedicalRecord_ShouldThrowBusinessRuleValidationException_WhenAppointmentIsNotInProgress()
    {
        // Arrange
        var scheduledAppointment = Appointment.Schedule(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
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
        var appointment = CreateAppointment(Guid.NewGuid());
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
    public void ValidateAndCompleteRecord_ShouldThrowDomainValidationException_WhenRecordIsNull()
    {
        // Act
        var act = () => _sut.ValidateAndCompleteRecord(null!, CreateValidContext());

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ValidateAndCompleteRecord_ShouldThrowDomainValidationException_WhenContextIsNull()
    {
        // Arrange
        var record = CreateMedicalRecord();

        // Act
        var act = () => _sut.ValidateAndCompleteRecord(record, null!);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ValidateAndCompleteRecord_ShouldThrowDomainValidationException_WhenExpectedDoctorIsNull()
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
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ValidateAndCompleteRecord_ShouldThrowDomainValidationException_WhenAppointmentIsNull()
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
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ValidateAndCompleteRecord_ShouldThrowDomainValidationException_WhenAppointmentTypeDefinitionIsNull()
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
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ValidateAndCompleteRecord_ShouldThrowBusinessRuleValidationException_WhenDoctorIdMismatch()
    {
        // Arrange
        var expectedDoctorId = Guid.NewGuid();
        var actualDoctorId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
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
        var doctorId = Guid.NewGuid();
        var expectedAppointmentId = Guid.NewGuid();
        var actualAppointmentId = Guid.NewGuid();
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
        var doctorId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
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

        record.ClinicalDetails.Should().HaveCount(2);
        record.ClinicalDetails.Should().Contain(detail1).And.Contain(detail2);
    }

    [Fact]
    public void AppendClinicalDetail_ShouldThrowDomainValidationException_WhenRecordIsNull()
    {
        var act = () =>
            _sut.AppendClinicalDetail(
                null!,
                DynamicClinicalDetail.Create("Test1", "{}"),
                CreateFormTemplate()
            );
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void AppendClinicalDetail_ShouldThrowDomainValidationException_WhenDetailIsNull()
    {
        var record = CreateMedicalRecord();
        var act = () => _sut.AppendClinicalDetail(record, null!, CreateFormTemplate());
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void AppendClinicalDetail_ShouldThrowDomainValidationException_WhenTemplateIsNull()
    {
        var record = CreateMedicalRecord();
        var act = () =>
            _sut.AppendClinicalDetail(record, DynamicClinicalDetail.Create("Test1", "{}"), null!);
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void AppendClinicalDetail_ShouldThrowBusinessRuleValidationException_WhenTemplateCodeMismatch()
    {
        var record = CreateMedicalRecord();
        var detail = DynamicClinicalDetail.Create("Test1", "{}");
        var template = CreateFormTemplate("DifferentCode");

        var act = () => _sut.AppendClinicalDetail(record, detail, template);

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
        var record = CreateMedicalRecord();
        var template = CreateFormTemplate("Test1");

        var detail = DynamicClinicalDetail.Create("Test1", payload!);

        var act = () => _sut.AppendClinicalDetail(record, detail, template);

        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.MedicalEncounter.MissingPayload);
    }

    [Fact]
    public void AppendClinicalDetail_ShouldThrowBusinessRuleValidationException_WhenPayloadIsInvalidSchema()
    {
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

        var act = () => _sut.AppendClinicalDetail(record, detail, template);

        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage($"{DomainErrors.MedicalEncounter.ValidationFailed}: {errorMessage}");
    }

    [Fact]
    public void AppendClinicalDetail_ShouldAddDetail_WhenValidAndSchemaMatches()
    {
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

        _sut.AppendClinicalDetail(record, detail, template);

        record.ClinicalDetails.Should().Contain(detail);
    }

    private MedicalEncounterContext CreateValidContext() =>
        new()
        {
            ExpectedDoctor = CreateDoctor(Guid.NewGuid()),
            Appointment = CreateAppointment(Guid.NewGuid()),
            AppointmentTypeDefinition = CreateAppointmentType(),
            CompletedAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

    private static MedicalRecord CreateMedicalRecord(Guid doctorId, Guid appointmentId) =>
        MedicalRecord.Create(Guid.NewGuid(), doctorId, appointmentId, "Headache");

    private static MedicalRecord CreateMedicalRecord() =>
        MedicalRecord.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Headache");

    private static Doctor CreateDoctor(Guid id)
    {
        var doctor = Doctor.Create(
            Guid.NewGuid(),
            PersonName.Create("Test Doctor"),
            MedicalLicenseNumber.Create("12345"),
            Guid.NewGuid(),
            "555-0000",
            ConsultationRoom.Create(1, "Room A", 1)
        );
        doctor.SetId(id);
        return doctor;
    }

    private Appointment CreateAppointment(Guid id)
    {
        var appointment = Appointment.Schedule(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
        );

        appointment.SetId(id);
        appointment.CheckIn(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime));
        appointment.Start(appointment.DoctorId, _fakeTime.GetUtcNow().UtcDateTime);

        return appointment;
    }

    private static AppointmentTypeDefinition CreateAppointmentType() =>
        AppointmentTypeDefinition.Create(
            Enums.AppointmentCategory.Checkup,
            "Checkup",
            "Desc",
            EncounterDuration.FromMinutes(30)
        );

    private static ClinicalFormTemplate CreateFormTemplate(
        string code = "Test1",
        string jsonSchema = "{}"
    ) => ClinicalFormTemplate.Create(code, "Test Form", "Desc", jsonSchema);
}
