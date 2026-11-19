using AwesomeAssertions;
using ClinicFlow.Infrastructure.Services;

namespace ClinicFlow.Infrastructure.Tests.Services;

public class HasherKeyDecoderTests
{
    private const int MinKeyLengthBytes = 32;

    private const string MissingMessage = "key is missing";
    private const string InvalidMessage = "key is invalid";
    private const string TooShortMessage = "key is too short";

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Decode_ShouldThrowInvalidOperationException_WhenKeyIsMissing(string? key)
    {
        // Arrange && Act
        var act = () =>
            HasherKeyDecoder.Decode(
                key!,
                MinKeyLengthBytes,
                MissingMessage,
                InvalidMessage,
                TooShortMessage
            );

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage(MissingMessage);
    }

    [Fact]
    public void Decode_ShouldThrowInvalidOperationException_WhenKeyIsNotBase64()
    {
        // Arrange && Act
        var act = () =>
            HasherKeyDecoder.Decode(
                "not base64!!",
                MinKeyLengthBytes,
                MissingMessage,
                InvalidMessage,
                TooShortMessage
            );

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage(InvalidMessage);
    }

    [Fact]
    public void Decode_ShouldThrowInvalidOperationException_WhenKeyIsTooShort()
    {
        // Arrange
        var key = Convert.ToBase64String(Enumerable.Repeat((byte)0xAB, 16).ToArray());

        // Act
        var act = () =>
            HasherKeyDecoder.Decode(
                key,
                MinKeyLengthBytes,
                MissingMessage,
                InvalidMessage,
                TooShortMessage
            );

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage(TooShortMessage);
    }

    [Fact]
    public void Decode_ShouldNotIncludeKeyValueInMessage_WhenKeyIsInvalid()
    {
        // Arrange
        var key = "not base64!!";

        // Act
        var act = () =>
            HasherKeyDecoder.Decode(
                key,
                MinKeyLengthBytes,
                MissingMessage,
                InvalidMessage,
                TooShortMessage
            );

        // Assert
        act.Should().Throw<InvalidOperationException>().Which.Message.Should().NotContain(key);
    }

    [Fact]
    public void Decode_ShouldReturnDecodedBytes_WhenKeyIsValid()
    {
        // Arrange
        var expected = Enumerable.Repeat((byte)0xAB, 32).ToArray();
        var key = Convert.ToBase64String(expected);

        // Act
        var result = HasherKeyDecoder.Decode(
            key,
            MinKeyLengthBytes,
            MissingMessage,
            InvalidMessage,
            TooShortMessage
        );

        // Assert
        result.Should().Equal(expected);
    }

    [Fact]
    public void Decode_ShouldReturnDecodedBytes_WhenKeyLengthEqualsMinimum()
    {
        // Arrange
        var expected = Enumerable.Repeat((byte)0xCD, MinKeyLengthBytes).ToArray();
        var key = Convert.ToBase64String(expected);

        // Act
        var result = HasherKeyDecoder.Decode(
            key,
            MinKeyLengthBytes,
            MissingMessage,
            InvalidMessage,
            TooShortMessage
        );

        // Assert
        result.Should().HaveCount(MinKeyLengthBytes).And.Equal(expected);
    }
}
