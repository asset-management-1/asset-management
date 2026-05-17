namespace Be.Haven.Tests.Shared.Be.Haven.Core.Helpers;

public sealed class ValidationHelperTests
{
    [Fact]
    public void Validate_Should_ThrowArgumentNullException_When_ObjectIsNull()
    {
        // Act
        var act = () => ValidationHelper.Validate<CoreSampleParentModel>(null);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The object to validate cannot be null.*");
    }

    [Fact]
    public void Validate_Should_ReturnValidationResultsForNestedObjects_When_ObjectGraphIsInvalid()
    {
        // Arrange
        var model = new CoreSampleParentModel
        {
            Child = new CoreSampleChildModel(),
            Children =
            [
                new CoreSampleChildModel
                {
                    Name = "Child without required code"
                }
            ]
        };

        // Act
        var result = ValidationHelper.Validate(model);

        // Assert
        result.Should().HaveCount(3);
        result.SelectMany(x => x.MemberNames).Should().Contain(["Name", "RequiredCode"]);
    }

    [Fact]
    public void Validate_Should_ReturnEmptyList_When_ObjectGraphIsValid()
    {
        // Arrange
        var model = new CoreSampleParentModel
        {
            Name = "Parent",
            Child = new CoreSampleChildModel
            {
                RequiredCode = "C1"
            },
            Children =
            [
                new CoreSampleChildModel
                {
                    RequiredCode = "C2"
                }
            ]
        };

        // Act
        var result = ValidationHelper.Validate(model);

        // Assert
        result.Should().BeEmpty();
    }
}
