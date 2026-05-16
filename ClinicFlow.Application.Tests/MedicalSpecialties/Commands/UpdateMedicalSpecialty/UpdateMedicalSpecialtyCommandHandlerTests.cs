using ClinicFlow.Application.MedicalSpecialties.Commands.UpdateMedicalSpecialty;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.MedicalSpecialties.Commands.UpdateMedicalSpecialty;

public class UpdateMedicalSpecialtyCommandHandlerTests
{
    private readonly Mock<IMedicalSpecialtyRepository> _medicalSpecialtyRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateMedicalSpecialtyCommandHandler _sut;

    public UpdateMedicalSpecialtyCommandHandlerTests()
    {
        _medicalSpecialtyRepositoryMock = new Mock<IMedicalSpecialtyRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new UpdateMedicalSpecialtyCommandHandler(
            _medicalSpecialtyRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldUpdateMedicalSpecialty_WhenValidCommand()
    {
        // Arrange
        var specialty = MedicalSpecialty.Create("Cardiology", "Heart specialty", 30, 24);
        var command = new UpdateMedicalSpecialtyCommand(
            specialty.Id,
            "Dermatology",
            "Skin specialty",
            45,
            12
        );

        _medicalSpecialtyRepositoryMock
            .Setup(x => x.GetByIdAsync(command.SpecialtyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(specialty);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        specialty.Name.Should().Be(command.Name);
        specialty.Description.Should().Be(command.Description);
        specialty.TypicalDuration.Minutes.Should().Be(command.TypicalDurationMinutes);
        specialty.CancellationPolicy.Hours.Should().Be(command.MinCancellationHours);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenSpecialtyNotFound()
    {
        // Arrange
        var command = new UpdateMedicalSpecialtyCommand(
            Guid.NewGuid(),
            "Dermatology",
            "Skin specialty",
            45,
            12
        );

        _medicalSpecialtyRepositoryMock
            .Setup(x => x.GetByIdAsync(command.SpecialtyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MedicalSpecialty?)null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(MedicalSpecialty));

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenNameAlreadyExists()
    {
        // Arrange
        var specialty = MedicalSpecialty.Create("Cardiology", "Heart specialty", 30, 24);
        var command = new UpdateMedicalSpecialtyCommand(
            specialty.Id,
            "Existing Name",
            "New description",
            45,
            12
        );

        _medicalSpecialtyRepositoryMock
            .Setup(x => x.GetByIdAsync(command.SpecialtyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(specialty);

        _medicalSpecialtyRepositoryMock
            .Setup(x =>
                x.ExistsByNameExcludingAsync(
                    command.Name,
                    command.SpecialtyId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.MedicalSpecialty.NameAlreadyExists);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
