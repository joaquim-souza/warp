using System.Diagnostics;
using Warp.Core.Audit;
using Warp.Core.Model;
using Warp.Core.Parsing;
using Warp.Core.Registry;
using Warp.Core.Serialization;
using Warp.Core.Templates;
using Warp.Core.Transform;

namespace Warp.Core.Engine;

public sealed class WarpEngine
{
    private readonly ParserRegistry _parsers;
    private readonly SerializerRegistry _serializers;
    private readonly Transformer _transformer;
    private readonly IAuditSink? _auditSink;

    public WarpEngine(
        ParserRegistry parsers,
        SerializerRegistry serializers,
        Transformer transformer,
        IAuditSink? auditSink = null)
    {
        _parsers = parsers;
        _serializers = serializers;
        _transformer = transformer;
        _auditSink = auditSink;
    }

    public WarpResult Execute(
        Stream input,
        Stream output,
        TemplateDefinition template)
    {
        var stopwatch =
            Stopwatch.StartNew();

        try
        {
            var parser =
                _parsers.Get(
                    template.SourceFormat);

            var serializer =
                _serializers.Get(
                    template.TargetFormat);

            var document =
                ParseInput(
                    parser,
                    input,
                    template);

            var transformation =
                _transformer.Transform(
                    document,
                    template);

            if (transformation.Validation.IsValid)
            {
                serializer.Serialize(
                    transformation.Output,
                    output);
            }

            stopwatch.Stop();

            RecordAudit(
                template,
                stopwatch.Elapsed,
                transformation.Validation.Errors.Count,
                transformation.Validation.IsValid);

            return new WarpResult(
                transformation.Validation);
        }
        catch
        {
            stopwatch.Stop();

            RecordAudit(
                template,
                stopwatch.Elapsed,
                1,
                false);

            throw;
        }
    }

    private static CanonicalDocument ParseInput(
        ICanonicalParser parser,
        Stream input,
        TemplateDefinition template)
    {
        if (parser is CsvParser csvParser)
        {
            return csvParser.Parse(
                input,
                template.Encoding);
        }

        return parser.Parse(
            input);
    }

    private void RecordAudit(
    TemplateDefinition template,
    TimeSpan duration,
    int validationErrorCount,
    bool success)
{
    if (_auditSink is null)
    {
        return;
    }

    var auditEvent =
        new WarpAuditEvent(
            template.Id,
            template.Version,
            template.SourceFormat,
            template.TargetFormat,
            success,
            validationErrorCount,
            duration);

    try
    {
        _auditSink.Record(
            auditEvent);
    }
    catch
    {
        // Auditoria é observabilidade.
        // Falha no sink nunca deve interromper
        // uma transformação válida.
    }
}
}