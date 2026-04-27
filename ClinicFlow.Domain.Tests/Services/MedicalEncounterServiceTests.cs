using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.Services.Policies;
using ClinicFlow.Domain.Tests.Shared;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
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
        var record = CreateMedicalRecord(Guid.NewGuid(), Guid.NewGuid());

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
        var record = CreateMedicalRecord(Guid.NewGuid(), Guid.NewGuid());

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
        var record = CreateMedicalRecord(Guid.NewGuid(), Guid.NewGuid());

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
        var record = CreateMedicalRecord(Guid.NewGuid(), Guid.NewGuid());

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
            Appointment = CreateAppointment(appointmentId, _fakeTime.GetUtcNow().UtcDateTime),
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
            Appointment = CreateAppointment(
                expectedAppointmentId,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
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
        var detailMock1 = new TestClinicalDetail1();
        var detailMock2 = new TestClinicalDetail2();
        var providedDetails = new List<IClinicalDetailRecord> { detailMock1, detailMock2 };

        var context = new MedicalEncounterContext
        {
            ExpectedDoctor = CreateDoctor(doctorId),
            Appointment = CreateAppointment(appointmentId, _fakeTime.GetUtcNow().UtcDateTime),
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
        record.ClinicalDetails.Should().Contain(detailMock1).And.Contain(detailMock2);
    }

    [Fact]
    public void AppendClinicalDetail_ShouldThrowDomainValidationException_WhenRecordIsNull()
    {
        var act = () =>
            _sut.AppendClinicalDetail(null!, new TestClinicalDetail1(), CreateFormTemplate());
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void AppendClinicalDetail_ShouldThrowDomainValidationException_WhenDetailIsNull()
    {
        var record = CreateMedicalRecord(Guid.NewGuid(), Guid.NewGuid());
        var act = () => _sut.AppendClinicalDetail(record, null!, CreateFormTemplate());
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void AppendClinicalDetail_ShouldThrowDomainValidationException_WhenTemplateIsNull()
    {
        var record = CreateMedicalRecord(Guid.NewGuid(), Guid.NewGuid());
        var act = () => _sut.AppendClinicalDetail(record, new TestClinicalDetail1(), null!);
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void AppendClinicalDetail_ShouldThrowBusinessRuleValidationException_WhenTemplateCodeMismatch()
    {
        var record = CreateMedicalRecord(Guid.NewGuid(), Guid.NewGuid());
        var detail = new TestClinicalDetail1();
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
        var record = CreateMedicalRecord(Guid.NewGuid(), Guid.NewGuid());
        var template = CreateFormTemplate("Test1");

        var mockDetail = new Mock<IClinicalDetailRecord>();
        mockDetail.Setup(d => d.TemplateCode).Returns("Test1");
        mockDetail.Setup(d => d.JsonDataPayload).Returns(payload!);

        var act = () => _sut.AppendClinicalDetail(record, mockDetail.Object, template);

        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.MedicalEncounter.MissingPayload);
    }

    [Fact]
    public void AppendClinicalDetail_ShouldThrowBusinessRuleValidationException_WhenPayloadIsInvalidSchema()
    {
        var record = CreateMedicalRecord(Guid.NewGuid(), Guid.NewGuid());

        var mockDetail = new Mock<IClinicalDetailRecord>();
        mockDetail.Setup(d => d.TemplateCode).Returns("Test1");
        mockDetail.Setup(d => d.JsonDataPayload).Returns("""{"invalid": "data"}""");

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

        var act = () => _sut.AppendClinicalDetail(record, mockDetail.Object, template);

        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage($"{DomainErrors.MedicalEncounter.ValidationFailed}: {errorMessage}");
    }

    [Fact]
    public void AppendClinicalDetail_ShouldAddDetail_WhenValidAndSchemaMatches()
    {
        var record = CreateMedicalRecord(Guid.NewGuid(), Guid.NewGuid());

        var mockDetail = new Mock<IClinicalDetailRecord>();
        mockDetail.Setup(d => d.TemplateCode).Returns("Test1");
        mockDetail.Setup(d => d.JsonDataPayload).Returns("""{"valid": "data"}""");

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

        _sut.AppendClinicalDetail(record, mockDetail.Object, template);

        record.ClinicalDetails.Should().Contain(mockDetail.Object);
    }

    private class TestClinicalDetail1 : IClinicalDetailRecord
    {
        public string TemplateCode => "Test1";
        public string JsonDataPayload => string.Empty;
    }

    private class TestClinicalDetail2 : IClinicalDetailRecord
    {
        public string TemplateCode => "Test2";
        public string JsonDataPayload => string.Empty;
    }

    private MedicalEncounterContext CreateValidContext() =>
        new()
        {
            ExpectedDoctor = CreateDoctor(Guid.NewGuid()),
            Appointment = CreateAppointment(Guid.NewGuid(), _fakeTime.GetUtcNow().UtcDateTime),
            AppointmentTypeDefinition = CreateAppointmentType(),
            CompletedAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

    private static MedicalRecord CreateMedicalRecord(Guid doctorId, Guid appointmentId)
    {
        var record = MedicalRecord.Create(Guid.NewGuid(), doctorId, appointmentId, "Headache");
        record.SetId(Guid.NewGuid());
        return record;
    }

    private static Doctor CreateDoctor(Guid id)
    {
        var doctor = Doctor.Create(
            Guid.NewGuid(),
            MedicalLicenseNumber.Create("12345"),
            Guid.NewGuid(),
            "555-0000",
            ConsultationRoom.Create(1, "Room A", 1)
        );
        doctor.SetId(id);
        return doctor;
    }

    private static Appointment CreateAppointment(Guid id, DateTime dt)
    {
        var appointment = Appointment.Schedule(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            dt.AddDays(1).Date,
            TimeRange.Create(new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0))
        );
        appointment.SetId(id);

        appointment.CheckIn(dt);
        appointment.Start(appointment.DoctorId, dt);

        return appointment;
    }

    private static AppointmentTypeDefinition CreateAppointmentType()
    {
        var type = AppointmentTypeDefinition.Create(
            Enums.AppointmentCategory.Checkup,
            "Checkup",
            "Desc",
            TimeSpan.FromMinutes(30)
        );
        type.SetId(Guid.NewGuid());
        return type;
    }

    private static ClinicalFormTemplate CreateFormTemplate(
        string code = "Test1",
        string jsonSchema = "{}"
    )
    {
        var template = ClinicalFormTemplate.Create(code, "Test Form", "Desc", jsonSchema);
        template.SetId(Guid.NewGuid());
        return template;
    }
}
