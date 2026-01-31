using ClinicFlow.Domain.Common;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.Common;

public class ValueObjectTests
{
    private class TestValueObject(int id, string name) : ValueObject
    {
        public int Id { get; } = id;
        public string Name { get; } = name;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
            yield return Name;
        }
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenValuesAreSame()
    {
        // Arrange
        var obj1 = new TestValueObject(1, "test");
        var obj2 = new TestValueObject(1, "test");

        // Act & Assert
        (obj1 == obj2).Should().BeTrue();
        (obj1 != obj2).Should().BeFalse();
        obj1.Equals(obj2).Should().BeTrue();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenValuesAreDifferent()
    {
        // Arrange
        var obj1 = new TestValueObject(1, "test");
        var obj2 = new TestValueObject(2, "test");

        // Act & Assert
        (obj1 == obj2).Should().BeFalse();
        (obj1 != obj2).Should().BeTrue();
        obj1.Equals(obj2).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenOneIsNull()
    {
        // Arrange
        var obj1 = new TestValueObject(1, "test");
        TestValueObject? obj2 = null;

        // Act & Assert
        (obj1 == obj2).Should().BeFalse();
        (obj1 != obj2).Should().BeTrue();
        obj1.Equals(obj2).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenBothAreNull()
    {
        // Arrange
        TestValueObject? obj1 = null;
        TestValueObject? obj2 = null;

        // Act & Assert
        (obj1 == obj2).Should().BeTrue();
        (obj1 != obj2).Should().BeFalse();
    }
}
