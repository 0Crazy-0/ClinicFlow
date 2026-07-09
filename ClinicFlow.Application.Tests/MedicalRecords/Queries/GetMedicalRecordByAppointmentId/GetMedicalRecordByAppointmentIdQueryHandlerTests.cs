using AwesomeAssertions;
using ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordByAppointmentId;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
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
        var appointmentId = Guid.CreateVersion7();
        var expectedRecord = MedicalRecord.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            appointmentId,
            "Headache"
        );

        _medicalRecordRepositoryMock
            .Setup(x =>
                x.GetByAppointmentIdAsync(appointmentId, TestContext.Current.CancellationToken)
            )
            .ReturnsAsync(expectedRecord);

        var query = new GetMedicalRecordByAppointmentIdQuery(appointmentId);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedRecord.Id);
        result.PatientId.Should().Be(expectedRecord.PatientId);
        result.DoctorId.Should().Be(expectedRecord.DoctorId);
        result.AppointmentId.Should().Be(expectedRecord.AppointmentId);
        result.ChiefComplaint.Should().Be(expectedRecord.ChiefComplaint);

        _medicalRecordRepositoryMock.Verify(
            x => x.GetByAppointmentIdAsync(appointmentId, TestContext.Current.CancellationToken),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_GivenInvalidAppointmentId_ThrowsEntityNotFoundException()
    {
        // Arrange
        var appointmentId = Guid.CreateVersion7();

        _medicalRecordRepositoryMock
            .Setup(x =>
                x.GetByAppointmentIdAsync(appointmentId, TestContext.Current.CancellationToken)
            )
            .ReturnsAsync((MedicalRecord?)null);

        var query = new GetMedicalRecordByAppointmentIdQuery(appointmentId);

        // Act
        var act = async () => await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(MedicalRecord));

        _medicalRecordRepositoryMock.Verify(
            x => x.GetByAppointmentIdAsync(appointmentId, TestContext.Current.CancellationToken),
            Times.Once
        );
    }
}
