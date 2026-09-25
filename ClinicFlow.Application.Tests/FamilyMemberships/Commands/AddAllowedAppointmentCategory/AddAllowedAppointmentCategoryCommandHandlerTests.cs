using AwesomeAssertions;
using ClinicFlow.Application.FamilyMemberships.Commands.AddAllowedAppointmentCategory;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.FamilyMemberships.Commands.AddAllowedAppointmentCategory;

public class AddAllowedAppointmentCategoryCommandHandlerTests
{
    private readonly Mock<IFamilyMembershipRepository> _familyMembershipRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly AddAllowedAppointmentCategoryCommandHandler _sut;

    public AddAllowedAppointmentCategoryCommandHandlerTests()
    {
        _sut = new AddAllowedAppointmentCategoryCommandHandler(
            _familyMembershipRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _fakeTime,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldAddCategoryAndSaveChanges_WhenValidCommand()
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

        var command = new AddAllowedAppointmentCategoryCommand(
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
                x.HasActiveSelfMembershipByUserIdAsync(
                    requesterUserId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        var requesterMembership = FamilyMembership.CreateFamilyMember(
            patient.Id,
            requesterUserId,
            PatientRelationship.Child,
            FamilyMembershipAccessLevel.Full,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patient.Id,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(requesterMembership);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        membership.AllowedAppointmentCategories.Should().Contain(command.Category);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldAddCategoryAndSaveChanges_WhenRequesterIsPatientsSelf()
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

        var command = new AddAllowedAppointmentCategoryCommand(
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
                x.HasActiveSelfMembershipByUserIdAsync(
                    requesterUserId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        var requesterMembership = FamilyMembership.CreateSelf(
            patient.Id,
            requesterUserId,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patient.Id,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(requesterMembership);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        membership.AllowedAppointmentCategories.Should().Contain(command.Category);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainValidationException_WhenRequesterHasNoActiveMembershipWithPatient()
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

        var command = new AddAllowedAppointmentCategoryCommand(
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
                x.HasActiveSelfMembershipByUserIdAsync(
                    requesterUserId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

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

        var command = new AddAllowedAppointmentCategoryCommand(
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

        var command = new AddAllowedAppointmentCategoryCommand(
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
