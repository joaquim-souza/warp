using Warp.Core.Templates;

namespace Warp.Core.Observability;

public interface ITransformationObserver
{
    void OnCompleted(
        TemplateDefinition template,
        TransformationMetrics metrics);
}