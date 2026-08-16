using System.Diagnostics;
using Warp.Core.Audit;
using Warp.Core.Registry;
using Warp.Core.Templates;
using Warp.Core.Transform;

namespace Warp.Core.Engine;

public sealed class WarpEngine
{
    private readonly ParserRegistry _parsers;
    private readonly SerializerRegistry _serializers;
    private readonly Transformer _transformer;
    private readonly IAuditSink _auditSink;

    public WarpEngine(
        ParserRegistry parsers,
        SerializerRegistry serializers,
        Transformer transformer,
        IAuditSink? auditSink = null)
    {
        _parsers = parsers;
        _serializers = serializers;
        _transformer = transformer;
        _auditSink = auditSink ?? new NullAuditSink();
    }

    public WarpResult Execute(
        Stream input,
        Stream output,
        TemplateDefinition template)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(template);

        var stopwatch = Stopwatch.StartNew();

        WarpResult result;

        try
        {
            var parser = _parsers.Get(template.SourceFormat);
            var serializer = _serializers.Get(template.TargetFormat);

            var source = parser.Parse(input);

            var (transformed, validation) =
                _transformer.Transform(source, template);

            if (!validation.IsValid)
            {
                result = new WarpResult(validation);
            }
            else
            {
                serializer.Serialize(transformed, output);
                result = WarpResult.Success();
            }
        }
        catch
        {
            stopwatch.Stop();

            RecordAudit(
                template,
                success: false,
                validationErrorCount: 0,
                stopwatch.Elapsed);

            throw;
        }

        stopwatch.Stop();

        RecordAudit(
            template,
            result.IsSuccess,
            result.Validation.Errors.Count,
            stopwatch.Elapsed);

        return result;
    }

    private void RecordAudit(
        TemplateDefinition template,
        bool success,
        int validationErrorCount,
        TimeSpan duration)
    {
        var auditEvent = new WarpAuditEvent(
            template.Id,
            template.Version,
            template.SourceFormat,
            template.TargetFormat,
            success,
            validationErrorCount,
            duration);

        // Auditoria não pode derrubar a transformação.
        try
        {
            _auditSink.Record(auditEvent);
        }
        catch
        {
            // O sink é infraestrutura opcional.
            // Falha de auditoria não altera o resultado do WARP.
        }
    }
}