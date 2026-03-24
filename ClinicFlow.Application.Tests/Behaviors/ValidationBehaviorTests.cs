using ClinicFlow.Application.Behaviors;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using ValidationException = ClinicFlow.Application.Exceptions.ValidationException;

namespace ClinicFlow.Application.Tests.Behaviors;

public class ValidationBehaviorTests
{
    public class DummyRequest : IRequest<Unit>
    {
        public string Value { get; set; } = string.Empty;
    }

    [Fact]
    public async Task Handle_ShouldReturnNext_WhenNoValidatorsPresent()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<DummyRequest>>();
        var _sut = new ValidationBehavior<DummyRequest, Unit>(validators);
        var request = new DummyRequest { Value = "Test" };
        var nextDelegateMock = new Mock<RequestHandlerDelegate<Unit>>();
        var expectedResponse = Unit.Value;
        nextDelegateMock.Setup(next => next()).ReturnsAsync(expectedResponse);

        // Act
        var result = await _sut.Handle(request, nextDelegateMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        nextDelegateMock.Verify(next => next(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNext_WhenValidationSucceeds()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<DummyRequest>>();
        validatorMock
            .Setup(v =>
                v.ValidateAsync(
                    It.IsAny<ValidationContext<DummyRequest>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new ValidationResult());

        var validators = new List<IValidator<DummyRequest>> { validatorMock.Object };
        var _sut = new ValidationBehavior<DummyRequest, Unit>(validators);
        var request = new DummyRequest { Value = "Test" };
        var nextDelegateMock = new Mock<RequestHandlerDelegate<Unit>>();
        var expectedResponse = Unit.Value;
        nextDelegateMock.Setup(next => next()).ReturnsAsync(expectedResponse);

        // Act
        var result = await _sut.Handle(request, nextDelegateMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        nextDelegateMock.Verify(next => next(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidationFails()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<DummyRequest>>();
        var validationFailure = new ValidationFailure("Value", "Value is invalid");
        validatorMock
            .Setup(v =>
                v.ValidateAsync(
                    It.IsAny<ValidationContext<DummyRequest>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new ValidationResult([validationFailure]));

        var validators = new List<IValidator<DummyRequest>> { validatorMock.Object };
        var _sut = new ValidationBehavior<DummyRequest, Unit>(validators);
        var request = new DummyRequest { Value = "Invalid" };
        var nextDelegateMock = new Mock<RequestHandlerDelegate<Unit>>();

        // Act
        var act = async () =>
            await _sut.Handle(request, nextDelegateMock.Object, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().ContainKey("Value");
        exception.Which.Errors["Value"].Should().Contain("Value is invalid");
        nextDelegateMock.Verify(next => next(), Times.Never);
    }
}
