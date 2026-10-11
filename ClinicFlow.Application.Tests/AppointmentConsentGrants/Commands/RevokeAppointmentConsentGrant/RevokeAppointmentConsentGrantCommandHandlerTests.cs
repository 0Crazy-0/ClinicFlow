using AwesomeAssertions;
using ClinicFlow.Application.AppointmentConsentGrants.Commands.RevokeAppointmentConsentGrant;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.AppointmentConsentGrants.Commands.RevokeAppointmentConsentGrant;

public class RevokeAppointmentConsentGrantCommandHandlerTests
{
    private readonly Mock<IAppointmentConsentGrantRepository> _consentGrantRepositoryMock = new();
    private readonly Mock<IFamilyMembershipRepository> _familyMembershipRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly RevokeAppointmentConsentGrantCommandHandler _sut;

    public RevokeAppointmentConsentGrantCommandHandlerTests()
    {
        _sut = new RevokeAppointmentConsentGrantCommandHandler(
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

        var command = new RevokeAppointmentConsentGrantCommand(Guid.CreateVersion7(), grant.Id);

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
        var command = new RevokeAppointmentConsentGrantCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7()
        );

        _consentGrantRepositoryMock
            .Setup(x => x.GetByIdAsync(command.ConsentGrantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppointmentConsentGrant?)null);

        // Act
        var act = () => _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(AppointmentConsentGrant));

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainValidationException_WhenRequesterIsNotAuthorized()
    {
        // Arrange
        var grant = CreateGrant();
        _fakeTime.Advance(TimeSpan.FromDays(10));
        var command = new RevokeAppointmentConsentGrantCommand(Guid.CreateVersion7(), grant.Id);

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
            .WithMessage(DomainErrors.AppointmentConsentGrant.UnauthorizedRevocation);

        grant.RevokedAt.Should().BeNull();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private AppointmentConsentGrant CreateGrant() =>
        AppointmentConsentGrant.CreateForCategory(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            ProtectedCategory.MentalHealthCounseling,
            Guid.CreateVersion7(),
            _fakeTime.GetUtcNow().UtcDateTime
        );
}
