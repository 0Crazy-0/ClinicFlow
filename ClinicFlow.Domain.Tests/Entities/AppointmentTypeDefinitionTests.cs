using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Appointments;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.Entities;

public class AppointmentTypeDefinitionTests
{
    // Create
    [Fact]
    public void Create_ShouldCreateInstance_WhenValidParameters()
    {
        // Arrange
        var type = AppointmentType.Checkup;
        var name = "General Checkup";
        var description = "Routine consultation";
        var duration = TimeSpan.FromMinutes(30);

        // Act
        var result = AppointmentTypeDefinition.Create(type, name, description, duration);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(type);
        result.Name.Should().Be(name);
        result.Description.Should().Be(description);
        result.DurationMinutes.Should().Be(duration);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenNameIsEmpty(string? name)
    {
        // Arrange & Act
        var act = () => AppointmentTypeDefinition.Create(AppointmentType.Checkup, name!, "Description", TimeSpan.FromMinutes(30));

        // Assert
        act.Should().Throw<InvalidAppointmentTypeException>().WithMessage("Appointment type name cannot be empty.");
    }

    [Theory]
    [MemberData(nameof(InvalidDurations))]
    public void Create_ShouldThrowException_WhenDurationIsZeroOrNegative(TimeSpan duration)
    {
        // Arrange & Act
        var act = () => AppointmentTypeDefinition.Create(AppointmentType.Checkup, "Checkup", "Description", duration);

        // Assert
        act.Should().Throw<InvalidAppointmentTypeException>().WithMessage("Duration must be positive.");
    }
    
    // Helper
    public static TheoryData<TimeSpan> InvalidDurations => [TimeSpan.Zero, TimeSpan.FromMinutes(-10)];
}
