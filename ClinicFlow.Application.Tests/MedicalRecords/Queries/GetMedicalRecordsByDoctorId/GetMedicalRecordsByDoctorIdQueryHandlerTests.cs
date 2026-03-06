using ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordsByDoctorId;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;
using System.Reflection;
using ClinicFlow.Domain.Entities.ClinicalDetails;

namespace ClinicFlow.Application.Tests.MedicalRecords.Queries.GetMedicalRecordsByDoctorId;

public class GetMedicalRecordsByDoctorIdQueryHandlerTests
{
    private readonly Mock<IMedicalRecordRepository> _medicalRecordRepositoryMock;
    private readonly GetMedicalRecordsByDoctorIdQueryHandler _sut;

    public GetMedicalRecordsByDoctorIdQueryHandlerTests()
    {
        _medicalRecordRepositoryMock = new Mock<IMedicalRecordRepository>();
        _sut = new GetMedicalRecordsByDoctorIdQueryHandler(_medicalRecordRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMedicalRecordDtos_WhenRecordsExistForDoctor()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var patientId1 = Guid.NewGuid();
        var patientId2 = Guid.NewGuid();
        var request = new GetMedicalRecordsByDoctorIdQuery(doctorId);

        var record1 = CreateMedicalRecord(Guid.NewGuid(), patientId1, doctorId, Guid.NewGuid(), "Headache");
        record1.AddClinicalDetail(DynamicClinicalDetail.Create("vital-signs", "{}"));

        var record2 = CreateMedicalRecord(Guid.NewGuid(), patientId2, doctorId, Guid.NewGuid(), "Fever");

        _medicalRecordRepositoryMock.Setup(x => x.GetByDoctorIdAsync(doctorId, CancellationToken.None)).ReturnsAsync([record1, record2]);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var firstResult = result.First(r => r.Id == record1.Id);
        firstResult.PatientId.Should().Be(patientId1);
        firstResult.DoctorId.Should().Be(doctorId);
        firstResult.ChiefComplaint.Should().Be("Headache");
        firstResult.ClinicalDetails.Should().ContainSingle();

        var secondResult = result.First(r => r.Id == record2.Id);
        secondResult.PatientId.Should().Be(patientId2);
        secondResult.DoctorId.Should().Be(doctorId);
        secondResult.ChiefComplaint.Should().Be("Fever");
        secondResult.ClinicalDetails.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoRecordsExistForDoctor()
    {
        // Arrange
        var request = new GetMedicalRecordsByDoctorIdQuery(Guid.NewGuid());

        _medicalRecordRepositoryMock.Setup(x => x.GetByDoctorIdAsync(request.DoctorId, CancellationToken.None)).ReturnsAsync([]);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
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
