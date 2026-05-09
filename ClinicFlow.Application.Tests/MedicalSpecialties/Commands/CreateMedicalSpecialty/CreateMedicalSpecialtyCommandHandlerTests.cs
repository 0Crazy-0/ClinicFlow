using ClinicFlow.Application.MedicalSpecialties.Commands.CreateMedicalSpecialty;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.MedicalSpecialties.Commands.CreateMedicalSpecialty;

public class CreateMedicalSpecialtyCommandHandlerTests
{
    private readonly Mock<IMedicalSpecialtyRepository> _medicalSpecialtyRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateMedicalSpecialtyCommandHandler _sut;

    public CreateMedicalSpecialtyCommandHandlerTests()
    {
        _medicalSpecialtyRepositoryMock = new Mock<IMedicalSpecialtyRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new CreateMedicalSpecialtyCommandHandler(
            _medicalSpecialtyRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateMedicalSpecialty_WhenValidCommand()
    {
        // Arrange
        var command = new CreateMedicalSpecialtyCommand("Cardiology", "Heart specialty", 30, 24);

        MedicalSpecialty? capturedSpecialty = null;
        _medicalSpecialtyRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<MedicalSpecialty>(), It.IsAny<CancellationToken>()))
            .Callback<MedicalSpecialty, CancellationToken>((s, _) => capturedSpecialty = s);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        capturedSpecialty.Should().NotBeNull();
        capturedSpecialty.Name.Should().Be(command.Name);
        capturedSpecialty.Description.Should().Be(command.Description);
        capturedSpecialty.TypicalDurationMinutes.Should().Be(command.TypicalDurationMinutes);
        capturedSpecialty.MinCancellationHours.Should().Be(command.MinCancellationHours);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryCreateAndSaveChanges_WhenValidCommand()
    {
        // Arrange
        var command = new CreateMedicalSpecialtyCommand("Cardiology", "Heart specialty", 30, 24);

        _medicalSpecialtyRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<MedicalSpecialty>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MedicalSpecialty s, CancellationToken _) => s);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _medicalSpecialtyRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<MedicalSpecialty>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenNameAlreadyExists()
    {
        // Arrange
        var command = new CreateMedicalSpecialtyCommand("Cardiology", "Heart specialty", 30, 24);

        _medicalSpecialtyRepositoryMock
            .Setup(x => x.ExistsByNameAsync(command.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.MedicalSpecialty.NameAlreadyExists);

        _medicalSpecialtyRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<MedicalSpecialty>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
