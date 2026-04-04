using ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordById;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

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

        var record = CreateMedicalRecord(patientId, doctorId, appointmentId, "Headache");
        var clinicalDetail = DynamicClinicalDetail.Create("vital-signs", "{}");
        record.AddClinicalDetail(clinicalDetail);

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(id, CancellationToken.None))
            .ReturnsAsync(record);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(record.Id);
        result.PatientId.Should().Be(patientId);
        result.DoctorId.Should().Be(doctorId);
        result.AppointmentId.Should().Be(appointmentId);
        result.ChiefComplaint.Should().Be("Headache");
        result.ClinicalDetails.Should().ContainSingle();
        result.ClinicalDetails[0].TemplateCode.Should().Be("vital-signs");
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenRecordDoesNotExist()
    {
        // Arrange
        var request = new GetMedicalRecordByIdQuery(Guid.NewGuid());

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(request.Id, CancellationToken.None))
            .ReturnsAsync((MedicalRecord?)null);

        // Act
        var act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(MedicalRecord));
    }

    private static MedicalRecord CreateMedicalRecord(
        Guid patientId,
        Guid doctorId,
        Guid appointmentId,
        string chiefComplaint
    ) => MedicalRecord.Create(patientId, doctorId, appointmentId, chiefComplaint);
}
