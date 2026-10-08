using AwesomeAssertions;
using ClinicFlow.Application.MedicalRecordConsentGrants.Commands.RevokeMedicalRecordConsentGrant;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.MedicalRecordConsentGrants.Commands.RevokeMedicalRecordConsentGrant;

public class RevokeMedicalRecordConsentGrantCommandHandlerTests
{
    private readonly Mock<IMedicalRecordConsentGrantRepository> _consentGrantRepositoryMock = new();
    private readonly Mock<IFamilyMembershipRepository> _familyMembershipRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly RevokeMedicalRecordConsentGrantCommandHandler _sut;

    public RevokeMedicalRecordConsentGrantCommandHandlerTests()
    {
        _sut = new RevokeMedicalRecordConsentGrantCommandHandler(
            _fakeTime,
            _consentGrantRepositoryMock.Object,
            _familyMembershipRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldRevokeGrantAndSaveChanges_WhenAuthorized()
    {
        // Arrange
        var grant = CreateGrant();
        _fakeTime.Advance(TimeSpan.FromDays(10));
        var revokeTime = _fakeTime.GetLocalNow().DateTime;
        var command = new RevokeMedicalRecordConsentGrantCommand(Guid.CreateVersion7(), grant.Id);

        _consentGrantRepositoryMock
            .Setup(x => x.GetByIdAsync(grant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grant);
        _familyMembershipRepositoryMock
            .Setup(x =>
                x.HasActiveSelfMembershipAsync(
                    command.RequesterUserId,
                    grant.PatientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        grant.RevokedAt.Should().Be(revokeTime);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenGrantDoesNotExist()
    {
        // Arrange
        var command = new RevokeMedicalRecordConsentGrantCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7()
        );

        _consentGrantRepositoryMock
            .Setup(x => x.GetByIdAsync(command.ConsentGrantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MedicalRecordConsentGrant?)null);

        // Act
        var act = () => _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(MedicalRecordConsentGrant));

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainValidationException_WhenRequesterIsNotAuthorized()
    {
        // Arrange
        var grant = CreateGrant();
        _fakeTime.Advance(TimeSpan.FromDays(10));
        var command = new RevokeMedicalRecordConsentGrantCommand(Guid.CreateVersion7(), grant.Id);

        _consentGrantRepositoryMock
            .Setup(x => x.GetByIdAsync(grant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grant);
        _familyMembershipRepositoryMock
            .Setup(x =>
                x.HasActiveSelfMembershipAsync(
                    command.RequesterUserId,
                    grant.PatientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);

        // Act
        var act = () => _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecordConsentGrant.UnauthorizedRevocation);

        grant.RevokedAt.Should().BeNull();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private MedicalRecordConsentGrant CreateGrant() =>
        MedicalRecordConsentGrant.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            _fakeTime.GetUtcNow().UtcDateTime
        );
}
