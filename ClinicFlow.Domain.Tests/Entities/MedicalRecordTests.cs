using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions.Base;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.Entities;

public class MedicalRecordTests
{
    // Create
    [Fact]
    public void Create_ShouldCreateMedicalRecord_WhenValidDataProvided()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var chiefComplaint = "Headache and fever";

        // Act
        var record = MedicalRecord.Create(patientId, doctorId, appointmentId, chiefComplaint);

        // Assert
        record.Should().NotBeNull();
        record.PatientId.Should().Be(patientId);
        record.DoctorId.Should().Be(doctorId);
        record.AppointmentId.Should().Be(appointmentId);
        record.ChiefComplaint.Should().Be(chiefComplaint);
        record.ClinicalDetails.Should().BeEmpty();
        record.DomainEvents.Should().ContainSingle(e => e is MedicalRecordCreatedEvent);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222", "Complaint", "Patient ID cannot be empty.")]
    [InlineData("11111111-1111-1111-1111-111111111111", "00000000-0000-0000-0000-000000000000", "22222222-2222-2222-2222-222222222222", "Complaint", "Doctor ID cannot be empty.")]
    [InlineData("11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222", "00000000-0000-0000-0000-000000000000", "Complaint", "Appointment ID cannot be empty.")]
    [InlineData("11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222", "33333333-3333-3333-3333-333333333333", "", "Chief complaint cannot be empty.")]
    [InlineData("11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222", "33333333-3333-3333-3333-333333333333", "   ", "Chief complaint cannot be empty.")]
    [InlineData("11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222", "33333333-3333-3333-3333-333333333333", null, "Chief complaint cannot be empty.")]
    public void Create_ShouldThrowException_WhenIdOrComplaintIsInvalid(string patientIdStr, string doctorIdStr, string appointmentIdStr, string? chiefComplaint, string expectedMessage)
    {
        // Arrange & Act
        var act = () => MedicalRecord.Create(Guid.Parse(patientIdStr), Guid.Parse(doctorIdStr), Guid.Parse(appointmentIdStr), chiefComplaint!);

        // Assert
        act.Should().Throw<DomainValidationException>().WithMessage(expectedMessage);
    }

    // AddClinicalDetail
    [Fact]
    public void AddClinicalDetail_ShouldAddDetail_WhenValid()
    {
        // Arrange
        var record = CreateValidMedicalRecord();
        var detail = new StubDynamicClinicalDetail("VITALS");

        // Act
        record.AddClinicalDetail(detail);

        // Assert
        record.ClinicalDetails.Should().ContainSingle();
        record.ClinicalDetails.First().Should().Be(detail);
    }

    [Fact]
    public void AddClinicalDetail_ShouldThrowException_WhenDetailIsNull()
    {
        // Arrange
        var record = CreateValidMedicalRecord();

        // Act
        var act = () => record.AddClinicalDetail(null!);

        // Assert
        act.Should().Throw<DomainValidationException>().WithMessage("Clinical detail cannot be null.");
    }

    [Fact]
    public void AddClinicalDetail_ShouldThrowException_WhenDuplicateDetailTypeAdded()
    {
        // Arrange
        var record = CreateValidMedicalRecord();
        var detail1 = new StubDynamicClinicalDetail("VITALS");
        var detail2 = new StubDynamicClinicalDetail("VITALS"); 

        record.AddClinicalDetail(detail1);

        // Act
        var act = () => record.AddClinicalDetail(detail2);

        // Assert
        act.Should().Throw<DomainValidationException>().WithMessage($"A clinical detail for template '{detail2.TemplateCode}' already exists in this medical record.");
    }

    // GetClinicalDetail
    [Fact]
    public void GetClinicalDetail_ShouldReturnDetail_WhenExists()
    {
        // Arrange
        var record = CreateValidMedicalRecord();
        var detail = new StubDynamicClinicalDetail("VITALS");
        record.AddClinicalDetail(detail);

        // Act
        var result = record.GetClinicalDetail("VITALS");

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(detail);
    }

    [Fact]
    public void GetClinicalDetail_ShouldReturnNull_WhenDoesNotExist()
    {
        // Arrange
        var record = CreateValidMedicalRecord();
        var detail = new StubDynamicClinicalDetail("VITALS");
        record.AddClinicalDetail(detail);

        // Act
        var result = record.GetClinicalDetail("ALLERGIES");

        // Assert
        result.Should().BeNull();
    }

    // Helpers
    private static MedicalRecord CreateValidMedicalRecord() => MedicalRecord.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "General checkup");


    private class StubDynamicClinicalDetail(string templateCode) : IClinicalDetailRecord
    {
        public string TemplateCode => templateCode;
    }
}
