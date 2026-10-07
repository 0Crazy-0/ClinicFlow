using AwesomeAssertions;
using ClinicFlow.Application.MedicalRecordConsentGrants.Queries.DTOs;
using ClinicFlow.Application.MedicalRecordConsentGrants.Queries.GetMedicalRecordConsentGrantsByPatientId;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.MedicalRecordConsentGrants.Queries.GetMedicalRecordConsentGrantsByPatientId;

public class GetMedicalRecordConsentGrantsByPatientIdQueryHandlerTests
{
    private readonly Mock<IFamilyMembershipRepository> _familyMembershipRepositoryMock = new();
    private readonly Mock<IMedicalRecordConsentGrantRepository> _consentGrantRepositoryMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly GetMedicalRecordConsentGrantsByPatientIdQueryHandler _sut;

    public GetMedicalRecordConsentGrantsByPatientIdQueryHandlerTests()
    {
        _sut = new GetMedicalRecordConsentGrantsByPatientIdQueryHandler(
            _familyMembershipRepositoryMock.Object,
            _consentGrantRepositoryMock.Object,
            _fakeTime
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedList_WhenGrantsExist()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var query = new GetMedicalRecordConsentGrantsByPatientIdQuery(
            requesterUserId,
            patientId,
            1,
            10
        );

        var effectiveGrant = CreateGrant(patientId);
        var revokedGrant = CreateGrant(patientId);

        _fakeTime.Advance(TimeSpan.FromDays(10));

        revokedGrant.Revoke(
            requesterIsAuthorized: true,
            referenceTime: _fakeTime.GetUtcNow().UtcDateTime
        );

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.HasActiveSelfMembershipAsync(
                    requesterUserId,
                    patientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        _consentGrantRepositoryMock
            .Setup(x =>
                x.GetByPatientIdPaginatedAsync(patientId, 1, 10, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                (new List<MedicalRecordConsentGrant> { effectiveGrant, revokedGrant }, 2)
            );

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var referenceTime = _fakeTime.GetLocalNow().DateTime;
        var expectedDtos = new List<MedicalRecordConsentGrant>
        {
            effectiveGrant,
            revokedGrant,
        }.Select(grant => new MedicalRecordConsentGrantDto(
            grant.Id,
            grant.PatientId,
            grant.RecipientMembershipId,
            grant.MedicalRecordId,
            grant.SignedByUserId,
            grant.SignedAt,
            grant.ExpiresAt,
            grant.RevokedAt,
            grant.IsEffectiveAt(referenceTime)
        ));

        result.Items.Should().BeEquivalentTo(expectedDtos);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(1);

        _familyMembershipRepositoryMock.Verify(
            x =>
                x.HasActiveSelfMembershipAsync(
                    requesterUserId,
                    patientId,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _consentGrantRepositoryMock.Verify(
            x => x.GetByPatientIdPaginatedAsync(patientId, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPaginatedList_WhenNoGrantsExist()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var query = new GetMedicalRecordConsentGrantsByPatientIdQuery(
            requesterUserId,
            patientId,
            1,
            10
        );

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.HasActiveSelfMembershipAsync(
                    requesterUserId,
                    patientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        _consentGrantRepositoryMock
            .Setup(x =>
                x.GetByPatientIdPaginatedAsync(patientId, 1, 10, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((new List<MedicalRecordConsentGrant>(), 0));

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(0);

        _familyMembershipRepositoryMock.Verify(
            x =>
                x.HasActiveSelfMembershipAsync(
                    requesterUserId,
                    patientId,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _consentGrantRepositoryMock.Verify(
            x => x.GetByPatientIdPaginatedAsync(patientId, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainValidationException_WhenRequesterHasNoActiveSelfMembership()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var query = new GetMedicalRecordConsentGrantsByPatientIdQuery(
            requesterUserId,
            patientId,
            1,
            10
        );

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.HasActiveSelfMembershipAsync(
                    requesterUserId,
                    patientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);

        // Act
        var act = () => _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecordConsentGrant.UnauthorizedAccess);

        _familyMembershipRepositoryMock.Verify(
            x =>
                x.HasActiveSelfMembershipAsync(
                    requesterUserId,
                    patientId,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _consentGrantRepositoryMock.Verify(
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

    private MedicalRecordConsentGrant CreateGrant(Guid patientId) =>
        MedicalRecordConsentGrant.Create(
            patientId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            _fakeTime.GetUtcNow().UtcDateTime
        );
}
