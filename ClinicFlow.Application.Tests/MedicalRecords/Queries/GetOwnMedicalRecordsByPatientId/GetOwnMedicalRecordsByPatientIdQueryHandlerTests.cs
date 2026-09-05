using AwesomeAssertions;
using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using ClinicFlow.Application.MedicalRecords.Queries.GetOwnMedicalRecordsByPatientId;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.MedicalRecords.Queries.GetOwnMedicalRecordsByPatientId;

public class GetOwnMedicalRecordsByPatientIdQueryHandlerTests
{
    private readonly Mock<IFamilyMembershipRepository> _familyMembershipRepositoryMock = new();
    private readonly Mock<IMedicalRecordRepository> _medicalRecordRepositoryMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly GetOwnMedicalRecordsByPatientIdQueryHandler _sut;

    public GetOwnMedicalRecordsByPatientIdQueryHandlerTests()
    {
        _sut = new GetOwnMedicalRecordsByPatientIdQueryHandler(
            _familyMembershipRepositoryMock.Object,
            _medicalRecordRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFullHistoryWithoutFilters_WhenMembershipRoleIsSelf()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var query = new GetOwnMedicalRecordsByPatientIdQuery(requesterUserId, patientId, 1, 10);

        var membership = FamilyMembership.CreateSelf(
            patientId,
            requesterUserId,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var record1 = CreateMedicalRecord(patientId);
        var record2 = CreateMedicalRecord(patientId);

        record1.AddClinicalDetail(DynamicClinicalDetail.Create("vital-signs", "{}"));

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(membership);

        _medicalRecordRepositoryMock
            .Setup(x =>
                x.GetByPatientIdPaginatedAsync(patientId, 1, 10, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(([record1, record2], 2));

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
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(1);

        _familyMembershipRepositoryMock.Verify(
            x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patientId,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _medicalRecordRepositoryMock.Verify(
            x => x.GetByPatientIdPaginatedAsync(patientId, 1, 10, It.IsAny<CancellationToken>()),
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

    [Theory]
    [InlineData(PatientRelationship.Child)]
    [InlineData(PatientRelationship.Spouse)]
    [InlineData(PatientRelationship.Sibling)]
    public async Task Handle_ShouldThrowUnauthorizedAccess_WhenMembershipRoleIsNotSelf(
        PatientRelationship role
    )
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var query = new GetOwnMedicalRecordsByPatientIdQuery(requesterUserId, patientId, 1, 10);

        var membership = FamilyMembership.CreateFamilyMember(
            patientId,
            requesterUserId,
            role,
            FamilyMembershipAccessLevel.Full,
            _fakeTime.GetUtcNow().UtcDateTime
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

        // Act
        var act = () => _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecord.UnauthorizedAccess);

        _familyMembershipRepositoryMock.Verify(
            x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patientId,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );

        _medicalRecordRepositoryMock.Verify(
            x =>
                x.GetByPatientIdPaginatedAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccess_WhenMembershipDoesNotExist()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var query = new GetOwnMedicalRecordsByPatientIdQuery(requesterUserId, patientId, 1, 10);

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
        await act.Should()
            .ThrowAsync<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecord.UnauthorizedAccess);

        _familyMembershipRepositoryMock.Verify(
            x =>
                x.GetActiveMembershipAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _medicalRecordRepositoryMock.Verify(
            x =>
                x.GetByPatientIdPaginatedAsync(
                    It.IsAny<Guid>(),
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
}
