using Warp.Core.Registry;
using Warp.Core.Templates;
using Warp.Core.Transform;

namespace Warp.Core.Engine;

public sealed class WarpEngine
{
    private readonly ParserRegistry _parsers;
    private readonly SerializerRegistry _serializers;
    private readonly Transformer _transformer;

    public WarpEngine(
        ParserRegistry parsers,
        SerializerRegistry serializers,
        Transformer transformer)
    {
        _parsers = parsers;
        _serializers = serializers;
        _transformer = transformer;
    }

    public WarpResult Execute(
        Stream input,
        Stream output,
        TemplateDefinition template)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(template);

        var parser = _parsers.Get(template.SourceFormat);
        var serializer = _serializers.Get(template.TargetFormat);

        var source = parser.Parse(input);

        var (transformed, validation) =
            _transformer.Transform(source, template);

        if (!validation.IsValid)
        {
            return new WarpResult(validation);
        }

        serializer.Serialize(transformed, output);

        return WarpResult.Success();
    }
}