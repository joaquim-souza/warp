namespace Warp.Core.Observability;

public sealed record TransformationMetrics(
    string TemplateId,
    int TemplateVersion,
    string SourceFormat,
    string TargetFormat,
    TimeSpan Duration,
    int ValidationErrorCount,
    bool Success);