using System.Reflection;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.Services.Policies;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Domain.Tests.Services;

public class MedicalEncounterServiceTests
{
    private readonly Mock<IMedicalRecordValidationPolicy> _mockPolicy1;
    private readonly Mock<IMedicalRecordValidationPolicy> _mockPolicy2;
    private readonly MedicalEncounterService _sut;

    public MedicalEncounterServiceTests()
    {
        _mockPolicy1 = new Mock<IMedicalRecordValidationPolicy>();
        _mockPolicy2 = new Mock<IMedicalRecordValidationPolicy>();

        var policies = new List<IMedicalRecordValidationPolicy> { _mockPolicy1.Object, _mockPolicy2.Object };
        _sut = new MedicalEncounterService(policies);
    }

    [Fact]
    public void ValidateAndCompleteRecord_ShouldThrowDomainValidationException_WhenRecordIsNull()
    {
        // Act
        var act = () => _sut.ValidateAndCompleteRecord(null!, new MedicalEncounterContext());

        // Assert
        act.Should().Throw<DomainValidationException>().WithMessage("The medical record is required and cannot be null.");
    }

    [Fact]
    public void ValidateAndCompleteRecord_ShouldThrowDomainValidationException_WhenContextIsNull()
    {
        // Arrange
        var record = CreateMedicalRecord(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var act = () => _sut.ValidateAndCompleteRecord(record, null!);

        // Assert
        act.Should().Throw<DomainValidationException>().WithMessage("The medical encounter context is required and cannot be null.");
    }

    [Fact]
    public void ValidateAndCompleteRecord_ShouldThrowBusinessRuleValidationException_WhenExpectedDoctorIsNull()
    {
        // Arrange
        var record = CreateMedicalRecord(Guid.NewGuid(), Guid.NewGuid());
        var context = new MedicalEncounterContext
        {
            ExpectedDoctor = null!,
            Appointment = CreateAppointment(Guid.NewGuid()),
            AppointmentTypeDefinition = CreateAppointmentType()
        };

        // Act
        var act = () => _sut.ValidateAndCompleteRecord(record, context);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>().WithMessage("Expected doctor context is missing.");
    }

    [Fact]
    public void ValidateAndCompleteRecord_ShouldThrowBusinessRuleValidationException_WhenAppointmentIsNull()
    {
        // Arrange
        var record = CreateMedicalRecord(Guid.NewGuid(), Guid.NewGuid());
        var context = new MedicalEncounterContext
        {
            ExpectedDoctor = CreateDoctor(Guid.NewGuid()),
            Appointment = null!,
            AppointmentTypeDefinition = CreateAppointmentType()
        };

        // Act
        var act = () => _sut.ValidateAndCompleteRecord(record, context);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>()
           .WithMessage("Appointment context is missing.");
    }

    [Fact]
    public void ValidateAndCompleteRecord_ShouldThrowBusinessRuleValidationException_WhenAppointmentTypeDefinitionIsNull()
    {
        // Arrange
        var record = CreateMedicalRecord(Guid.NewGuid(), Guid.NewGuid());
        var context = new MedicalEncounterContext
        {
            ExpectedDoctor = CreateDoctor(Guid.NewGuid()),
            Appointment = CreateAppointment(Guid.NewGuid()),
            AppointmentTypeDefinition = null!
        };

        // Act
        var act = () => _sut.ValidateAndCompleteRecord(record, context);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>().WithMessage("Appointment type definition context is missing.");
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
            AppointmentTypeDefinition = CreateAppointmentType()
        };

        // Act
        var act = () => _sut.ValidateAndCompleteRecord(record, context);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>().WithMessage("The doctor provided does not match the doctor assigned to the medical record.");
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
            AppointmentTypeDefinition = CreateAppointmentType()
        };

        // Act
        var act = () => _sut.ValidateAndCompleteRecord(record, context);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>().WithMessage("The appointment provided does not match the appointment assigned to the medical record.");
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
            Appointment = CreateAppointment(appointmentId),
            AppointmentTypeDefinition = appointmentType,
            ProvidedDetails = providedDetails
        };

        // Act
        _sut.ValidateAndCompleteRecord(record, context);

        // Assert
        _mockPolicy1.Verify(p => p.Validate(appointmentType, providedDetails), Times.Once);
        _mockPolicy2.Verify(p => p.Validate(appointmentType, providedDetails), Times.Once);

        record.ClinicalDetails.Should().HaveCount(2);
        record.ClinicalDetails.Should().Contain(detailMock1).And.Contain(detailMock2);
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

    // Helpers

    private static MedicalRecord CreateMedicalRecord(Guid doctorId, Guid appointmentId)
    {
        var record = (MedicalRecord)Activator.CreateInstance(typeof(MedicalRecord), true)!;
        SetPrivateProperty(record, nameof(MedicalRecord.Id), Guid.NewGuid());
        SetPrivateProperty(record, nameof(MedicalRecord.DoctorId), doctorId);
        SetPrivateProperty(record, nameof(MedicalRecord.AppointmentId), appointmentId);
        return record;
    }

    private static Doctor CreateDoctor(Guid id)
    {
        var doctor = (Doctor)Activator.CreateInstance(typeof(Doctor), true)!;
        SetPrivateProperty(doctor, nameof(Doctor.Id), id);
        return doctor;
    }

    private static Appointment CreateAppointment(Guid id)
    {
        var appointment = (Appointment)Activator.CreateInstance(typeof(Appointment), true)!;
        SetPrivateProperty(appointment, nameof(Appointment.Id), id);
        return appointment;
    }

    private static AppointmentTypeDefinition CreateAppointmentType()
    {
        var type = (AppointmentTypeDefinition)Activator.CreateInstance(typeof(AppointmentTypeDefinition), true)!;
        SetPrivateProperty(type, nameof(AppointmentTypeDefinition.Id), Guid.NewGuid());
        return type;
    }

    private static void SetPrivateProperty(object obj, string propertyName, object value)
    {
        var type = obj.GetType();

        while (type != null)
        {
            var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (prop != null)
            {
                prop.SetValue(obj, value);
                return;
            }

            type = type.BaseType;
        }
    }
}
