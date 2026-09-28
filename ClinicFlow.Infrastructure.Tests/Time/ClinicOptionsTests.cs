using System.Text;
using AwesomeAssertions;
using ClinicFlow.Infrastructure.Time;
using Microsoft.Extensions.Configuration;

namespace ClinicFlow.Infrastructure.Tests.Time;

public class ClinicOptionsTests
{
    [Fact]
    public void FromConfiguration_ShouldReturnBoundOptions_WhenSectionExists()
    {
        // Arrange
        var configuration = CreateConfiguration("""{"Clinic":{"TimeZoneId":"Europe/Madrid"}}""");

        // Act
        var options = ClinicOptions.FromConfiguration(configuration);

        // Assert
        options.TimeZoneId.Should().Be("Europe/Madrid");
    }

    [Fact]
    public void FromConfiguration_ShouldReturnDefaultOptions_WhenSectionIsMissing()
    {
        // Arrange
        var configuration = CreateConfiguration("""{}""");

        // Act
        var options = ClinicOptions.FromConfiguration(configuration);

        // Assert
        options.Should().NotBeNull();
        options.TimeZoneId.Should().Be(new ClinicOptions().TimeZoneId);
    }

    private static IConfiguration CreateConfiguration(string json) =>
        new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
            .Build();
}
