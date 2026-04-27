using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.ValueObjects;

public class ConsultationRoomTests
{
    [Fact]
    public void Create_ShouldCreateConsultationRoom_WhenValidParameters()
    {
        // Arrange
        var number = 1;
        var name = "Cardiology A";
        var floor = 3;

        // Act
        var room = ConsultationRoom.Create(number, name, floor);

        // Assert
        room.Number.Should().Be(number);
        room.Name.Should().Be(name);
        room.Floor.Should().Be(floor);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_ShouldThrowException_WhenNumberIsZeroOrNegative(int number)
    {
        // Arrange & Act
        var act = () => ConsultationRoom.Create(number, "Room A", 1);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueMustBePositive);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenNameIsEmpty(string? name)
    {
        // Arrange & Act
        var act = () => ConsultationRoom.Create(1, name!, 1);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_ShouldThrowException_WhenFloorIsZeroOrNegative(int floor)
    {
        // Arrange & Act
        var act = () => ConsultationRoom.Create(1, "Room A", floor);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueMustBePositive);
    }

    [Fact]
    public void Create_ShouldTrimName()
    {
        // Arrange & Act
        var room = ConsultationRoom.Create(1, "  Cardiology A  ", 1);

        // Assert
        room.Name.Should().Be("Cardiology A");
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var room = ConsultationRoom.Create(1, "Cardiology A", 3);

        // Act
        var result = room.ToString();

        // Assert
        result.Should().Be("Room 1 - Cardiology A (Floor 3)");
    }
}
