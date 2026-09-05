using AwesomeAssertions;
using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using ClinicFlow.Application.MedicalRecords.Queries.GetFamilyMemberMedicalRecordsByPatientId;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.MedicalRecords.Queries.GetFamilyMemberMedicalRecordsByPatientId;

public class GetFamilyMemberMedicalRecordsByPatientIdQueryHandlerTests
{
    private readonly Mock<IFamilyMembershipRepository> _familyMembershipRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IMedicalRecordRepository> _medicalRecordRepositoryMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly GetFamilyMemberMedicalRecordsByPatientIdQueryHandler _sut;

    public GetFamilyMemberMedicalRecordsByPatientIdQueryHandlerTests()
    {
        _sut = new GetFamilyMemberMedicalRecordsByPatientIdQueryHandler(
            _familyMembershipRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _medicalRecordRepositoryMock.Object,
            _fakeTime
        );
    }

    [Fact]
    public async Task Handle_ShouldExcludeNoCategories_WhenPatientIsAdult()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patient = CreatePatientWithAge(30);
        var query = new GetFamilyMemberMedicalRecordsByPatientIdQuery(
            requesterUserId,
            patient.Id,
            1,
            10
        );

        var membership = CreateMembership(
            patient.Id,
            requesterUserId,
            FamilyMembershipAccessLevel.Full
        );

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

        IReadOnlyCollection<ProtectedCategory>? capturedExcludedCategories = null;

        _medicalRecordRepositoryMock
            .Setup(x =>
                x.GetByPatientIdPaginatedExcludingCategoriesAsync(
                    patient.Id,
                    It.IsAny<IReadOnlyCollection<ProtectedCategory>>(),
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback<Guid, IReadOnlyCollection<ProtectedCategory>, int, int, CancellationToken>(
                (_, excluded, _, _, _) => capturedExcludedCategories = excluded
            )
            .ReturnsAsync((new List<MedicalRecord>(), 0));

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(0);

        capturedExcludedCategories.Should().NotBeNull();
        capturedExcludedCategories.Should().BeEmpty();

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
        _medicalRecordRepositoryMock.Verify(
            x =>
                x.GetByPatientIdPaginatedExcludingCategoriesAsync(
                    patient.Id,
                    It.IsAny<IReadOnlyCollection<ProtectedCategory>>(),
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldExcludeEveryProtectedCategory_WhenPatientIsOneBelowAgeOfMajority()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patient = CreatePatientWithAge(17);
        var query = new GetFamilyMemberMedicalRecordsByPatientIdQuery(
            requesterUserId,
            patient.Id,
            1,
            10
        );

        var membership = CreateMembership(
            patient.Id,
            requesterUserId,
            FamilyMembershipAccessLevel.Full
        );

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

        IReadOnlyCollection<ProtectedCategory>? capturedExcludedCategories = null;

        _medicalRecordRepositoryMock
            .Setup(x =>
                x.GetByPatientIdPaginatedExcludingCategoriesAsync(
                    patient.Id,
                    It.IsAny<IReadOnlyCollection<ProtectedCategory>>(),
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback<Guid, IReadOnlyCollection<ProtectedCategory>, int, int, CancellationToken>(
                (_, excluded, _, _, _) => capturedExcludedCategories = excluded
            )
            .ReturnsAsync((new List<MedicalRecord>(), 0));

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().BeEmpty();

        capturedExcludedCategories.Should().NotBeNull();
        capturedExcludedCategories.Should().BeEquivalentTo(Enum.GetValues<ProtectedCategory>());
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
        var patient = CreatePatientWithAge(13);
        var query = new GetFamilyMemberMedicalRecordsByPatientIdQuery(
            requesterUserId,
            patient.Id,
            1,
            10
        );

        var membership = CreateMembership(patient.Id, requesterUserId, accessLevel);

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

        _patientRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _medicalRecordRepositoryMock.Verify(
            x =>
                x.GetByPatientIdPaginatedExcludingCategoriesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IReadOnlyCollection<ProtectedCategory>>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedRecords_WhenAccessLevelAllowsReading()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patient = CreatePatientWithAge(13);
        var query = new GetFamilyMemberMedicalRecordsByPatientIdQuery(
            requesterUserId,
            patient.Id,
            2,
            10
        );

        var membership = CreateMembership(
            patient.Id,
            requesterUserId,
            FamilyMembershipAccessLevel.Full
        );

        var record1 = CreateMedicalRecord(patient.Id);
        var record2 = CreateMedicalRecord(patient.Id);

        record1.AddClinicalDetail(DynamicClinicalDetail.Create("vital-signs", "{}"));

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

        _medicalRecordRepositoryMock
            .Setup(x =>
                x.GetByPatientIdPaginatedExcludingCategoriesAsync(
                    patient.Id,
                    It.IsAny<IReadOnlyCollection<ProtectedCategory>>(),
                    2,
                    10,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<MedicalRecord> { record1, record2 }, 2));

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDtos = new List<MedicalRecord> { record1, record2 }.Select(
            record => new MedicalRecordDto(
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
            )
        );

        result.Items.Should().BeEquivalentTo(expectedDtos);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(2);
        result.TotalPages.Should().Be(1);

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
        _medicalRecordRepositoryMock.Verify(
            x =>
                x.GetByPatientIdPaginatedExcludingCategoriesAsync(
                    patient.Id,
                    It.IsAny<IReadOnlyCollection<ProtectedCategory>>(),
                    2,
                    10,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenMembershipDoesNotExist()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var query = new GetFamilyMemberMedicalRecordsByPatientIdQuery(
            requesterUserId,
            patientId,
            1,
            10
        );

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patientId,
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

        _familyMembershipRepositoryMock.Verify(
            x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patientId,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _patientRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _medicalRecordRepositoryMock.Verify(
            x =>
                x.GetByPatientIdPaginatedExcludingCategoriesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IReadOnlyCollection<ProtectedCategory>>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenPatientDoesNotExist()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var query = new GetFamilyMemberMedicalRecordsByPatientIdQuery(
            requesterUserId,
            patientId,
            1,
            10
        );

        var membership = CreateMembership(
            patientId,
            requesterUserId,
            FamilyMembershipAccessLevel.Full
        );

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(membership);

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var act = () => _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Patient));

        _patientRepositoryMock.Verify(
            x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _medicalRecordRepositoryMock.Verify(
            x =>
                x.GetByPatientIdPaginatedExcludingCategoriesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IReadOnlyCollection<ProtectedCategory>>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    private static MedicalRecord CreateMedicalRecord(Guid patientId) =>
        MedicalRecord.Create(
            patientId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "chiefComplaint",
            null
        );

    private Patient CreatePatientWithAge(int age)
    {
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;

        return Patient.CreateProfile(
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(referenceTime.AddYears(-age)),
            referenceTime
        );
    }

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
