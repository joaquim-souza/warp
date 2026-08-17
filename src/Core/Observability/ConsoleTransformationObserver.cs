using Warp.Core.Templates;

namespace Warp.Core.Observability;

public sealed class ConsoleTransformationObserver
    : ITransformationObserver
{
    public void OnCompleted(
        TemplateDefinition template,
        TransformationMetrics metrics)
    {
        var status =
            metrics.Success
                ? "SUCCESS"
                : "FAILURE";

        Console.WriteLine(
            $"[WARP] {status} " +
            $"template={metrics.TemplateId} " +
            $"version={metrics.TemplateVersion} " +
            $"source={metrics.SourceFormat} " +
            $"target={metrics.TargetFormat} " +
            $"duration_ms={metrics.Duration.TotalMilliseconds:F2} " +
            $"validation_errors={metrics.ValidationErrorCount}");
    }
}