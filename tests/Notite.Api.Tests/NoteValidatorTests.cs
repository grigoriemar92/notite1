namespace Notite.Api.Tests;

public class NoteValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyText_IsInvalid(string? text)
    {
        var result = NoteValidator.Validate(text);

        Assert.False(result.IsValid);
        Assert.NotNull(result.Error);
    }

    [Theory]
    [InlineData("Cumpara lapte")]
    [InlineData("  text cu spatii  ")]
    public void Validate_NormalText_IsValid(string text)
    {
        var result = NoteValidator.Validate(text);

        Assert.True(result.IsValid);
        Assert.Null(result.Error);
    }
}
