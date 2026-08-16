namespace Warp.Core.Engine;

using Warp.Core.Transform;

public sealed class WarpResult
{
    public bool IsSuccess { get; }

    public ValidationResult Validation { get; }

    private WarpResult(
        bool isSuccess,
        ValidationResult validation)
    {
        IsSuccess = isSuccess;
        Validation = validation;
    }

    public static WarpResult Success() =>
        new(true, ValidationResult.Success());

    public WarpResult(ValidationResult validation)
    {
        Validation = validation;
        IsSuccess = validation.IsValid;
    }
}