namespace Warp.Core.Transform;

public sealed class ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<ValidationError> Errors { get; } = new();

    public static ValidationResult Success() => new();

    public void AddError(string path, string message) =>
        Errors.Add(new ValidationError(path, message));
}