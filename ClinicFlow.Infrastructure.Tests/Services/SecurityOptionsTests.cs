using System.Text;
using AwesomeAssertions;
using ClinicFlow.Infrastructure.Persistence.Options;
using Microsoft.Extensions.Configuration;

namespace ClinicFlow.Infrastructure.Tests.Services;

public class SecurityOptionsTests
{
    [Fact]
    public void FromConfiguration_ShouldReturnBoundOptions_WhenSectionExists()
    {
        // Arrange
        // Production DependencyInjection rejects this key when constructing SsnHasher.
        var configuration = CreateConfiguration("""{"Security":{"SsnHashKey":"abc"}}""");

        // Act
        var options = SecurityOptions.FromConfiguration(configuration);

        // Assert
        options.SsnHashKey.Should().Be("abc");
    }

    [Fact]
    public void FromConfiguration_ShouldReturnDefaultOptions_WhenSectionIsMissing()
    {
        // Arrange
        var configuration = CreateConfiguration("""{}""");

        // Act
        var options = SecurityOptions.FromConfiguration(configuration);

        // Assert
        options.Should().NotBeNull();
        options.SsnHashKey.Should().BeEmpty();
    }

    private static IConfiguration CreateConfiguration(string json) =>
        new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
            .Build();
}
