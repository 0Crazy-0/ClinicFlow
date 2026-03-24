using System.Reflection;
using ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordByAppointmentId;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.MedicalRecords.Queries.GetMedicalRecordByAppointmentId;

public class GetMedicalRecordByAppointmentIdQueryHandlerTests
{
    private readonly Mock<IMedicalRecordRepository> _medicalRecordRepositoryMock;
    private readonly GetMedicalRecordByAppointmentIdQueryHandler _sut;

    public GetMedicalRecordByAppointmentIdQueryHandlerTests()
    {
        _medicalRecordRepositoryMock = new Mock<IMedicalRecordRepository>();
        _sut = new GetMedicalRecordByAppointmentIdQueryHandler(_medicalRecordRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_GivenValidAppointmentId_ReturnsMedicalRecordDto()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var expectedRecord = CreateMedicalRecord(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            appointmentId,
            "Headache"
        );

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByAppointmentIdAsync(appointmentId, CancellationToken.None))
            .ReturnsAsync(expectedRecord);

        var query = new GetMedicalRecordByAppointmentIdQuery(appointmentId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedRecord.Id);
        result.PatientId.Should().Be(expectedRecord.PatientId);
        result.DoctorId.Should().Be(expectedRecord.DoctorId);
        result.AppointmentId.Should().Be(expectedRecord.AppointmentId);
        result.ChiefComplaint.Should().Be(expectedRecord.ChiefComplaint);

        _medicalRecordRepositoryMock.Verify(
            x => x.GetByAppointmentIdAsync(appointmentId, CancellationToken.None),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_GivenInvalidAppointmentId_ThrowsEntityNotFoundException()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByAppointmentIdAsync(appointmentId, CancellationToken.None))
            .ReturnsAsync((MedicalRecord?)null);

        var query = new GetMedicalRecordByAppointmentIdQuery(appointmentId);

        // Act
        var act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .Where(e => e.EntityName == nameof(MedicalRecord));
        _medicalRecordRepositoryMock.Verify(
            x => x.GetByAppointmentIdAsync(appointmentId, CancellationToken.None),
            Times.Once
        );
    }

    private static MedicalRecord CreateMedicalRecord(
        Guid id,
        Guid patientId,
        Guid doctorId,
        Guid appointmentId,
        string chiefComplaint
    )
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
            var prop = type.GetProperty(
                propertyName,
                BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.Instance
                    | BindingFlags.DeclaredOnly
            );
            if (prop != null)
            {
                prop.SetValue(obj, value);
                return;
            }
            type = type.BaseType;
        }
    }
}
