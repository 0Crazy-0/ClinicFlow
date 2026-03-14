using ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordById;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;
using System.Reflection;
using ClinicFlow.Domain.Entities.ClinicalDetails;

namespace ClinicFlow.Application.Tests.MedicalRecords.Queries.GetMedicalRecordById;

public class GetMedicalRecordByIdQueryHandlerTests
{
    private readonly Mock<IMedicalRecordRepository> _medicalRecordRepositoryMock;
    private readonly GetMedicalRecordByIdQueryHandler _sut;

    public GetMedicalRecordByIdQueryHandlerTests()
    {
        _medicalRecordRepositoryMock = new Mock<IMedicalRecordRepository>();
        _sut = new GetMedicalRecordByIdQueryHandler(_medicalRecordRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMedicalRecordDto_WhenRecordExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var request = new GetMedicalRecordByIdQuery(id);
        
        var record = CreateMedicalRecord(id, patientId, doctorId, appointmentId, "Headache");
        var clinicalDetail = DynamicClinicalDetail.Create("vital-signs", "{}");
        record.AddClinicalDetail(clinicalDetail);

        _medicalRecordRepositoryMock.Setup(x => x.GetByIdAsync(id, CancellationToken.None)).ReturnsAsync(record);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(id);
        result.PatientId.Should().Be(patientId);
        result.DoctorId.Should().Be(doctorId);
        result.AppointmentId.Should().Be(appointmentId);
        result.ChiefComplaint.Should().Be("Headache");
        result.ClinicalDetails.Should().ContainSingle();
        result.ClinicalDetails.First().TemplateCode.Should().Be("vital-signs");
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenRecordDoesNotExist()
    {
        // Arrange
        var request = new GetMedicalRecordByIdQuery(Guid.NewGuid());

        _medicalRecordRepositoryMock.Setup(x => x.GetByIdAsync(request.Id, CancellationToken.None)).ReturnsAsync((MedicalRecord?)null);

        // Act & Assert
        var action = async () => await _sut.Handle(request, CancellationToken.None);
        await action.Should().ThrowAsync<EntityNotFoundException>().Where(e => e.EntityName == nameof(MedicalRecord));
    }

    // Helpers
    private static MedicalRecord CreateMedicalRecord(Guid id, Guid patientId, Guid doctorId, Guid appointmentId, string chiefComplaint)
    {
        var record = (MedicalRecord)Activator.CreateInstance(typeof(MedicalRecord), true)!;
        SetPrivateProperty(record, nameof(MedicalRecord.Id), id);
        SetPrivateProperty(record, nameof(MedicalRecord.PatientId), patientId);
        SetPrivateProperty(record, nameof(MedicalRecord.DoctorId), doctorId);
        SetPrivateProperty(record, nameof(MedicalRecord.AppointmentId), appointmentId);
        SetPrivateProperty(record, nameof(MedicalRecord.ChiefComplaint), chiefComplaint);
        return record;
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
