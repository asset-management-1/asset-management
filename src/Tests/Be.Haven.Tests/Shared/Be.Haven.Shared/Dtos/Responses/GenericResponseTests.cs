namespace Be.Haven.Tests.Shared.Be.Haven.Shared.Dtos.Responses;

public sealed class GenericResponseTests
{
    [Fact]
    public void Constructor_Should_MarkSuccess_When_DataIsProvided()
    {
        // Arrange
        var model = new CoreSerializationModel
        {
            DisplayName = "Haven"
        };

        // Act
        var result = new GenericResponse<CoreSerializationModel>(model);

        // Assert
        result.Data.Should().BeSameAs(model);
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void Constructor_Should_KeepDefaultState_When_NoArgumentsAreProvided()
    {
        // Act
        var result = new GenericResponse<string>();

        // Assert
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
        result.ErrorCode.Should().Be(0);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Constructor_Should_MarkFailure_When_ErrorIsProvided()
    {
        // Arrange
        const string expectedMessage = "failed";

        // Act
        var result = new GenericResponse<string>(expectedMessage, 400);

        // Assert
        result.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorCode.Should().Be(400);
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ErrorMessage_Should_UpdateState_When_SetAfterSuccess()
    {
        // Arrange
        var sut = new GenericResponse<string>("ok");

        // Act
        sut.ErrorMessage = "failed";

        // Assert
        sut.IsSuccess.Should().BeFalse();
        sut.IsFailure.Should().BeTrue();
    }
}
