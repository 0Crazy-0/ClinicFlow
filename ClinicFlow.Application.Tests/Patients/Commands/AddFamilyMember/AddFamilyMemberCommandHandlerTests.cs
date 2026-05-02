using ClinicFlow.Application.Patients.Commands.AddFamilyMember;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Patients.Commands.AddFamilyMember;

public class AddFamilyMemberCommandHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FakeTimeProvider _fakeTime;
    private readonly AddFamilyMemberCommandHandler _sut;

    public AddFamilyMemberCommandHandlerTests()
    {
        _patientRepositoryMock = new Mock<IPatientRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fakeTime = new FakeTimeProvider();
        _sut = new AddFamilyMemberCommandHandler(
            _fakeTime,
            _patientRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateFamilyMember_WhenValidCommand()
    {
        // Arrange
        var command = new AddFamilyMemberCommand(
            Guid.NewGuid(),
            "Child",
            "Doe",
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-5).Date,
            PatientRelationship.Child
        );

        _patientRepositoryMock
            .Setup(x =>
                x.GetIncludingDeletedByNameAndDobAsync(
                    command.UserId,
                    It.IsAny<PersonName>(),
                    command.DateOfBirth,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((Patient?)null);

        Patient? capturedPatient = null;
        _patientRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()))
            .Callback<Patient, CancellationToken>((p, _) => capturedPatient = p)
            .ReturnsAsync((Patient p, CancellationToken _) => p);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        _patientRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        result.Should().NotBeEmpty();
        capturedPatient.Should().NotBeNull();
        capturedPatient.UserId.Should().Be(command.UserId);
        capturedPatient.FullName.ToString().Should().Be($"{command.FirstName} {command.LastName}");
        capturedPatient.RelationshipToUser.Should().Be(command.Relationship);
        capturedPatient.DateOfBirth.Should().Be(command.DateOfBirth);
    }

    [Fact]
    public async Task Handle_ShouldReactivateFamilyMember_WhenDeletedProfileExists()
    {
        // Arrange
        var command = new AddFamilyMemberCommand(
            Guid.NewGuid(),
            "Child",
            "Doe",
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-5).Date,
            PatientRelationship.Child
        );

        var deletedMember = Patient.CreateFamilyMember(
            command.UserId,
            PersonName.Create($"{command.FirstName} {command.LastName}"),
            PatientRelationship.Sibling,
            command.DateOfBirth,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        deletedMember.MarkAsDeleted();

        _patientRepositoryMock
            .Setup(x =>
                x.GetIncludingDeletedByNameAndDobAsync(
                    command.UserId,
                    It.IsAny<PersonName>(),
                    command.DateOfBirth,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(deletedMember);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(deletedMember.Id);
        deletedMember.IsDeleted.Should().BeFalse();
        deletedMember.RelationshipToUser.Should().Be(PatientRelationship.Child);

        _patientRepositoryMock.Verify(
            r => r.CreateAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
