using AwesomeAssertions;
using ClinicFlow.Application.Patients.Commands.RemoveFamilyMember;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Patients.Commands.RemoveFamilyMember;

public class RemoveFamilyMemberCommandHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FakeTimeProvider _fakeTime;
    private readonly RemoveFamilyMemberCommandHandler _sut;

    public RemoveFamilyMemberCommandHandlerTests()
    {
        _patientRepositoryMock = new Mock<IPatientRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fakeTime = new FakeTimeProvider();
        _sut = new RemoveFamilyMemberCommandHandler(
            _patientRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldMarkAsDeleted_WhenPatientIsFamilyMember()
    {
        // Arrange
        var command = new RemoveFamilyMemberCommand(Guid.CreateVersion7(), Guid.CreateVersion7());

        var familyMember = Patient.CreateFamilyMember(
            command.UserId,
            PersonName.Create("Family Member"),
            PatientRelationship.Child,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-10)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(command.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(familyMember);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        familyMember.IsDeleted.Should().BeTrue();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenPatientIsPrimaryUser()
    {
        // Arrange
        var command = new RemoveFamilyMemberCommand(Guid.CreateVersion7(), Guid.CreateVersion7());

        var primaryPatient = Patient.CreateSelf(
            command.UserId,
            PersonName.Create("Primary User"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(command.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(primaryPatient);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.CannotRemovePrimaryUser);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenPatientNotFound()
    {
        // Arrange
        var command = new RemoveFamilyMemberCommand(Guid.CreateVersion7(), Guid.CreateVersion7());

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(command.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Patient));

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserIsUnauthorized()
    {
        // Arrange
        var patientUserId = Guid.CreateVersion7();
        var command = new RemoveFamilyMemberCommand(Guid.CreateVersion7(), Guid.CreateVersion7());

        var familyMember = Patient.CreateFamilyMember(
            patientUserId,
            PersonName.Create("Family Member"),
            PatientRelationship.Child,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-10)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(command.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(familyMember);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedRemoval);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
