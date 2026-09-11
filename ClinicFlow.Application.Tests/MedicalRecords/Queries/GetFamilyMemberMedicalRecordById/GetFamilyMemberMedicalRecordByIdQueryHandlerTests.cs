using AwesomeAssertions;
using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using ClinicFlow.Application.MedicalRecords.Queries.GetFamilyMemberMedicalRecordById;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.MedicalRecords.Queries.GetFamilyMemberMedicalRecordById;

public class GetFamilyMemberMedicalRecordByIdQueryHandlerTests
{
    private readonly Mock<IMedicalRecordRepository> _medicalRecordRepositoryMock = new();
    private readonly Mock<IFamilyMembershipRepository> _familyMembershipRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly GetFamilyMemberMedicalRecordByIdQueryHandler _sut;

    public GetFamilyMemberMedicalRecordByIdQueryHandlerTests()
    {
        _sut = new GetFamilyMemberMedicalRecordByIdQueryHandler(
            _medicalRecordRepositoryMock.Object,
            _familyMembershipRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _fakeTime
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnMedicalRecordDto_WhenAccessAllowedAndNotProtected()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patient = CreatePatient();
        var record = CreateMedicalRecord(patient.Id);

        record.AddClinicalDetail(DynamicClinicalDetail.Create("vital-signs", "{}"));

        var query = new GetFamilyMemberMedicalRecordByIdQuery(requesterUserId, record.Id);
        var membership = CreateMembership(
            patient.Id,
            requesterUserId,
            FamilyMembershipAccessLevel.Full
        );

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patient.Id,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(membership);

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDto = new MedicalRecordDto(
            record.Id,
            record.PatientId,
            record.DoctorId,
            record.AppointmentId,
            record.ChiefComplaint,
            [
                .. record.ClinicalDetails.Select(d => new ClinicalDetailDto(
                    d.TemplateCode,
                    d.JsonDataPayload
                )),
            ]
        );

        result.Should().BeEquivalentTo(expectedDto);

        _medicalRecordRepositoryMock.Verify(
            x => x.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _familyMembershipRepositoryMock.Verify(
            x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patient.Id,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _patientRepositoryMock.Verify(
            x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenRecordDoesNotExist()
    {
        // Arrange
        var query = new GetFamilyMemberMedicalRecordByIdQuery(
            Guid.CreateVersion7(),
            Guid.CreateVersion7()
        );

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(query.MedicalRecordId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MedicalRecord?)null);

        // Act
        var act = () => _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(MedicalRecord));

        _medicalRecordRepositoryMock.Verify(
            x => x.GetByIdAsync(query.MedicalRecordId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _familyMembershipRepositoryMock.Verify(
            x =>
                x.GetActiveMembershipAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        _patientRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenMembershipDoesNotExist()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patient = CreatePatient();
        var record = CreateMedicalRecord(patient.Id);
        var query = new GetFamilyMemberMedicalRecordByIdQuery(requesterUserId, record.Id);

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patient.Id,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((FamilyMembership?)null);

        // Act
        var act = () => _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(FamilyMembership));

        _medicalRecordRepositoryMock.Verify(
            x => x.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _familyMembershipRepositoryMock.Verify(
            x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patient.Id,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _patientRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Theory]
    [InlineData(FamilyMembershipAccessLevel.Restricted)]
    [InlineData(FamilyMembershipAccessLevel.EmergencyOnly)]
    [InlineData(FamilyMembershipAccessLevel.AppointmentOnly)]
    public async Task Handle_ShouldThrowUnauthorizedAccess_WhenAccessLevelDoesNotAllowReading(
        FamilyMembershipAccessLevel accessLevel
    )
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patient = CreatePatient();
        var record = CreateMedicalRecord(patient.Id);
        var query = new GetFamilyMemberMedicalRecordByIdQuery(requesterUserId, record.Id);
        var membership = CreateMembership(patient.Id, requesterUserId, accessLevel);

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patient.Id,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(membership);

        // Act
        var act = () => _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecord.UnauthorizedAccess);

        _medicalRecordRepositoryMock.Verify(
            x => x.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _familyMembershipRepositoryMock.Verify(
            x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patient.Id,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _patientRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenPatientDoesNotExist()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patient = CreatePatient();
        var record = CreateMedicalRecord(patient.Id);
        var query = new GetFamilyMemberMedicalRecordByIdQuery(requesterUserId, record.Id);
        var membership = CreateMembership(
            patient.Id,
            requesterUserId,
            FamilyMembershipAccessLevel.Full
        );

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patient.Id,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(membership);

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var act = () => _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Patient));

        _medicalRecordRepositoryMock.Verify(
            x => x.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _familyMembershipRepositoryMock.Verify(
            x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patient.Id,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _patientRepositoryMock.Verify(
            x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccess_WhenRecordIsProtectedByMinorConsent()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patient = Patient.CreateProfile(
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-13)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var record = MedicalRecord.Create(
            patient.Id,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "chiefComplaint",
            ProtectedCategory.MentalHealthCounseling,
            null
        );

        var query = new GetFamilyMemberMedicalRecordByIdQuery(requesterUserId, record.Id);
        var membership = CreateMembership(
            patient.Id,
            requesterUserId,
            FamilyMembershipAccessLevel.Full
        );

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patient.Id,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(membership);

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        // Act
        var act = () => _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecord.ProtectedByMinorConsent);

        _medicalRecordRepositoryMock.Verify(
            x => x.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _familyMembershipRepositoryMock.Verify(
            x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patient.Id,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _patientRepositoryMock.Verify(
            x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    private static MedicalRecord CreateMedicalRecord(Guid patientId) =>
        MedicalRecord.Create(
            patientId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "chiefComplaint",
            null,
            null
        );

    private Patient CreatePatient() =>
        Patient.CreateProfile(
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

    private FamilyMembership CreateMembership(
        Guid patientId,
        Guid userId,
        FamilyMembershipAccessLevel accessLevel
    ) =>
        FamilyMembership.CreateFamilyMember(
            patientId,
            userId,
            PatientRelationship.Child,
            accessLevel,
            _fakeTime.GetUtcNow().UtcDateTime
        );
}
