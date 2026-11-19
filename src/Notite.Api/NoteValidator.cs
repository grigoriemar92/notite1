namespace Notite.Api;

public record ValidationResult(bool IsValid, string? Error)
{
    public static ValidationResult Ok() => new(true, null);
    public static ValidationResult Fail(string error) => new(false, error);
}

public static class NoteValidator
{
    public static ValidationResult Validate(string? text) =>
        string.IsNullOrWhiteSpace(text)
            ? ValidationResult.Fail("Textul notitei nu poate fi gol.")
            : ValidationResult.Ok();
}
