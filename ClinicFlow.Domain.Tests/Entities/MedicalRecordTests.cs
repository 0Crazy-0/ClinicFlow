using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.Entities;

public class MedicalRecordTests
{
    // Create
    [Fact]
    public void Create_ShouldCreateMedicalRecord_WhenValidParameters()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var chiefComplaint = "Persistent headache";

        // Act
        var record = MedicalRecord.Create(patientId, doctorId, appointmentId, chiefComplaint);

        // Assert
        record.Should().NotBeNull();
        record.PatientId.Should().Be(patientId);
        record.DoctorId.Should().Be(doctorId);
        record.AppointmentId.Should().Be(appointmentId);
        record.ChiefComplaint.Should().Be(chiefComplaint);
        record.Diagnosis.Should().BeEmpty();
        record.Treatment.Should().BeEmpty();
        record.Medications.Should().BeEmpty();
        record.LabResults.Should().BeEmpty();
        record.DoctorNotes.Should().BeEmpty();
        record.FollowUpInstructions.Should().BeEmpty();
    }

    [Fact]
    public void Create_ShouldEmitMedicalRecordCreatedEvent()
    {
        // Arrange & Act
        var record = CreateValidRecord();

        // Assert
        record.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<MedicalRecordCreatedEvent>().Which.MedicalRecord.Should().Be(record);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222", "Patient ID cannot be empty.")]
    [InlineData("11111111-1111-1111-1111-111111111111", "00000000-0000-0000-0000-000000000000", "22222222-2222-2222-2222-222222222222", "Doctor ID cannot be empty.")]
    [InlineData("11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222", "00000000-0000-0000-0000-000000000000", "Appointment ID cannot be empty.")]
    public void Create_ShouldThrowException_WhenIdIsEmpty(string patientIdStr, string doctorIdStr, string appointmentIdStr, string expectedMessage)
    {
        // Arrange & Act
        var act = () => MedicalRecord.Create(Guid.Parse(patientIdStr), Guid.Parse(doctorIdStr), Guid.Parse(appointmentIdStr), "Headache");

        // Assert
        act.Should().Throw<InvalidMedicalRecordException>().WithMessage(expectedMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenChiefComplaintIsEmpty(string? chiefComplaint)
    {
        // Arrange & Act
        var act = () => MedicalRecord.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), chiefComplaint!);

        // Assert
        act.Should().Throw<InvalidMedicalRecordException>().WithMessage("Chief complaint cannot be empty.");
    }

    //AddDiagnosis
    [Fact]
    public void AddDiagnosis_ShouldUpdateDiagnosis()
    {
        // Arrange
        var record = CreateValidRecord();

        // Act
        record.AddDiagnosis("Migraine");

        // Assert
        record.Diagnosis.Should().Be("Migraine");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddDiagnosis_ShouldThrowException_WhenDiagnosisIsEmpty(string? diagnosis)
    {
        // Arrange
        var record = CreateValidRecord();

        // Act
        var act = () => record.AddDiagnosis(diagnosis!);

        // Assert
        act.Should().Throw<InvalidMedicalRecordException>().WithMessage("Diagnosis cannot be empty.");
    }

    //PrescribeTreatment
    [Fact]
    public void PrescribeTreatment_ShouldUpdateTreatmentAndMedications()
    {
        // Arrange
        var record = CreateValidRecord();

        // Act
        record.PrescribeTreatment("Rest and hydration", "Ibuprofen 400mg");

        // Assert
        record.Treatment.Should().Be("Rest and hydration");
        record.Medications.Should().Be("Ibuprofen 400mg");
    }

    [Theory]
    [InlineData(null, "Ibuprofen", "Treatment cannot be empty.")]
    [InlineData("", "Ibuprofen", "Treatment cannot be empty.")]
    [InlineData("Rest", null, "Medications cannot be empty.")]
    [InlineData("Rest", "", "Medications cannot be empty.")]
    public void PrescribeTreatment_ShouldThrowException_WhenParametersAreInvalid(string? treatment, string? medications, string expectedMessage)
    {
        // Arrange
        var record = CreateValidRecord();

        // Act
        var act = () => record.PrescribeTreatment(treatment!, medications!);

        // Assert
        act.Should().Throw<InvalidMedicalRecordException>().WithMessage(expectedMessage);
    }

    //RecordLabResults
    [Fact]
    public void RecordLabResults_ShouldUpdateLabResults()
    {
        // Arrange
        var record = CreateValidRecord();

        // Act
        record.RecordLabResults("Blood count: normal");

        // Assert
        record.LabResults.Should().Be("Blood count: normal");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RecordLabResults_ShouldThrowException_WhenLabResultsAreEmpty(string? labResults)
    {
        // Arrange
        var record = CreateValidRecord();

        // Act
        var act = () => record.RecordLabResults(labResults!);

        // Assert
        act.Should().Throw<InvalidMedicalRecordException>().WithMessage("Lab results cannot be empty.");
    }

    //AddDoctorNotes
    [Fact]
    public void AddDoctorNotes_ShouldUpdateNotes()
    {
        // Arrange
        var record = CreateValidRecord();

        // Act
        record.AddDoctorNotes("Patient shows improvement");

        // Assert
        record.DoctorNotes.Should().Be("Patient shows improvement");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddDoctorNotes_ShouldThrowException_WhenNotesAreEmpty(string? notes)
    {
        // Arrange
        var record = CreateValidRecord();

        // Act
        var act = () => record.AddDoctorNotes(notes!);

        // Assert
        act.Should().Throw<InvalidMedicalRecordException>().WithMessage("Doctor notes cannot be empty.");
    }

    //SetFollowUpInstructions

    [Fact]
    public void SetFollowUpInstructions_ShouldUpdateInstructions()
    {
        // Arrange
        var record = CreateValidRecord();

        // Act
        record.SetFollowUpInstructions("Return in 2 weeks");

        // Assert
        record.FollowUpInstructions.Should().Be("Return in 2 weeks");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetFollowUpInstructions_ShouldThrowException_WhenInstructionsAreEmpty(string? instructions)
    {
        // Arrange
        var record = CreateValidRecord();

        // Act
        var act = () => record.SetFollowUpInstructions(instructions!);

        // Assert
        act.Should().Throw<InvalidMedicalRecordException>().WithMessage("Follow-up instructions cannot be empty.");
    }

    // Helper
    private static MedicalRecord CreateValidRecord() => MedicalRecord.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Persistent headache");
}
