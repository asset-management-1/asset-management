using Be.Haven.Core.Behaviors;
using Mediator;

namespace Be.Haven.Tests.Shared.Be.Haven.Core.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_Should_CallNext_When_NoValidatorsAreRegistered()
    {
        // Arrange
        var request = new CoreValidationRequest("Haven");
        var sut = new ValidationBehavior<CoreValidationRequest, string>([]);

        // Act
        var result = await sut.Handle(request, Next, CancellationToken.None);

        // Assert
        result.Should().Be("next:Haven");
    }

    [Fact]
    public async Task Handle_Should_CallNext_When_AllValidatorsPass()
    {
        // Arrange
        var request = new CoreValidationRequest("Haven");
        var sut = new ValidationBehavior<CoreValidationRequest, string>(
        [
            new CoreValidationRequestValidator()
        ]);

        // Act
        var result = await sut.Handle(request, Next, CancellationToken.None);

        // Assert
        result.Should().Be("next:Haven");
    }

    [Fact]
    public async Task Handle_Should_ThrowValidationException_When_ValidatorFails()
    {
        // Arrange
        var request = new CoreValidationRequest(string.Empty);
        var sut = new ValidationBehavior<CoreValidationRequest, string>(
        [
            new CoreValidationRequestValidator()
        ]);

        // Act
        var action = () => sut.Handle(request, Next, CancellationToken.None).AsTask();

        // Assert
        await action.Should().ThrowAsync<ValidationException>()
            .Where(x => x.Errors.Any(error => error.Field == nameof(CoreValidationRequest.Name)));
    }

    private static ValueTask<string> Next(CoreValidationRequest request, CancellationToken cancellationToken) =>
        ValueTask.FromResult($"next:{request.Name}");

    private sealed record CoreValidationRequest(string Name) : IRequest<string>;

    private sealed class CoreValidationRequestValidator : AbstractValidator<CoreValidationRequest>
    {
        public CoreValidationRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithErrorCode("name_required");
        }
    }
}
