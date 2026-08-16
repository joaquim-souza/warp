namespace Warp.Core.Transform;

public sealed record ValidationError(string Path, string Message);