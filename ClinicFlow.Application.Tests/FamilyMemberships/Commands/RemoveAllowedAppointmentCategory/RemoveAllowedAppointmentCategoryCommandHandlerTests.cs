using AwesomeAssertions;
using ClinicFlow.Application.FamilyMemberships.Commands.RemoveAllowedAppointmentCategory;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.FamilyMemberships.Commands.RemoveAllowedAppointmentCategory;

public class RemoveAllowedAppointmentCategoryCommandHandlerTests
{
    private readonly Mock<IFamilyMembershipRepository> _familyMembershipRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly RemoveAllowedAppointmentCategoryCommandHandler _sut;

    public RemoveAllowedAppointmentCategoryCommandHandlerTests()
    {
        _sut = new RemoveAllowedAppointmentCategoryCommandHandler(
            _familyMembershipRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _fakeTime,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldRemoveCategoryAndSaveChanges_WhenRequesterIsPatientsSelf()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var targetUserId = Guid.CreateVersion7();
        var patient = Patient.CreateProfile(
            PersonName.Create("Jane Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-25)),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var membership = CreateRestrictedMembership(patient.Id, targetUserId);
        membership.AddAllowedAppointmentCategory(
            AppointmentCategory.Pediatrics,
            requesterIsAuthorized: true
        );

        var command = new RemoveAllowedAppointmentCategoryCommand(
            requesterUserId,
            targetUserId,
            patient.Id,
            AppointmentCategory.Pediatrics
        );

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(targetUserId, patient.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(membership);

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.HasActiveSelfMembershipAsync(
                    requesterUserId,
                    patient.Id,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        membership.AllowedAppointmentCategories.Should().BeEmpty();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainValidationException_WhenPatientIsMinor()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var targetUserId = Guid.CreateVersion7();
        var patient = Patient.CreateProfile(
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-10)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var membership = CreateRestrictedMembership(patient.Id, targetUserId);
        membership.AddAllowedAppointmentCategory(
            AppointmentCategory.Pediatrics,
            requesterIsAuthorized: true
        );

        var command = new RemoveAllowedAppointmentCategoryCommand(
            requesterUserId,
            targetUserId,
            patient.Id,
            AppointmentCategory.Pediatrics
        );

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(targetUserId, patient.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(membership);

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.HasActiveSelfMembershipAsync(
                    requesterUserId,
                    patient.Id,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        // Act
        var act = () => _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.UnauthorizedCategoryListChange);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenTargetMembershipDoesNotExist()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var targetUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();

        var command = new RemoveAllowedAppointmentCategoryCommand(
            requesterUserId,
            targetUserId,
            patientId,
            AppointmentCategory.Pediatrics
        );

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(targetUserId, patientId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((FamilyMembership?)null);

        // Act
        var act = () => _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(FamilyMembership));

        _familyMembershipRepositoryMock.Verify(
            x =>
                x.GetActiveMembershipAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _patientRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenPatientDoesNotExist()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var targetUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var membership = CreateRestrictedMembership(patientId, targetUserId);

        var command = new RemoveAllowedAppointmentCategoryCommand(
            requesterUserId,
            targetUserId,
            patientId,
            AppointmentCategory.Pediatrics
        );

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(targetUserId, patientId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(membership);

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var act = () => _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Patient));

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private FamilyMembership CreateRestrictedMembership(Guid patientId, Guid ownerUserId) =>
        FamilyMembership.CreateFamilyMember(
            patientId,
            ownerUserId,
            PatientRelationship.Child,
            FamilyMembershipAccessLevel.Restricted,
            _fakeTime.GetUtcNow().UtcDateTime
        );
}
