using Warp.Core.Audit;
using Warp.Core.Engine;
using Warp.Core.Model;
using Warp.Core.Parsing;
using Warp.Core.Registry;
using Warp.Core.Serialization;
using Warp.Core.Templates;
using Warp.Core.Transform;

namespace Tests.Audit;

public sealed class WarpAuditTests
{
    [Fact]
    public void Execute_ShouldRecordSuccessfulExecution()
    {
        var auditSink = new RecordingAuditSink();

        var engine = CreateEngine(auditSink);

        var root = new CanonicalNode("root");
        root.AddChild("name", "Keyboard");

        var source = new MemoryStream();

        using var input = new MemoryStream(
            System.Text.Encoding.UTF8.GetBytes(
                """{"name":"Keyboard"}"""));

        using var output = new MemoryStream();

        var template = new TemplateDefinition
        {
            Id = "json-to-xml",
            Version = 1,
            SourceFormat = "json",
            TargetFormat = "xml",
            Mappings =
            [
                new FieldMapping
                {
                    SourcePath = "name",
                    TargetPath = "product.name",
                    Required = true
                }
            ]
        };

        var result = engine.Execute(
            input,
            output,
            template);

        Assert.True(result.IsSuccess);

        var audit = Assert.Single(auditSink.Events);

        Assert.Equal("json-to-xml", audit.TemplateId);
        Assert.Equal(1, audit.TemplateVersion);
        Assert.Equal("json", audit.SourceFormat);
        Assert.Equal("xml", audit.TargetFormat);
        Assert.True(audit.Success);
        Assert.Equal(0, audit.ValidationErrorCount);
        Assert.True(audit.Duration >= TimeSpan.Zero);
    }

    [Fact]
    public void Execute_ShouldRecordValidationFailure()
    {
        var auditSink = new RecordingAuditSink();

        var engine = CreateEngine(auditSink);

        using var input = new MemoryStream(
            System.Text.Encoding.UTF8.GetBytes(
                """{}"""));

        using var output = new MemoryStream();

        var template = new TemplateDefinition
        {
            Id = "required-field-test",
            Version = 1,
            SourceFormat = "json",
            TargetFormat = "xml",
            Mappings =
            [
                new FieldMapping
                {
                    SourcePath = "required",
                    TargetPath = "required",
                    Required = true
                }
            ]
        };

        var result = engine.Execute(
            input,
            output,
            template);

        Assert.False(result.IsSuccess);

        var audit = Assert.Single(auditSink.Events);

        Assert.Equal(
            "required-field-test",
            audit.TemplateId);

        Assert.False(audit.Success);
        Assert.Equal(1, audit.ValidationErrorCount);
        Assert.True(audit.Duration >= TimeSpan.Zero);
    }

    [Fact]
    public void AuditSinkFailure_ShouldNotBreakSuccessfulExecution()
    {
        var auditSink = new ThrowingAuditSink();

        var engine = CreateEngine(auditSink);

        using var input = new MemoryStream(
            System.Text.Encoding.UTF8.GetBytes(
                """{"name":"Keyboard"}"""));

        using var output = new MemoryStream();

        var template = new TemplateDefinition
        {
            Id = "audit-failure-test",
            Version = 1,
            SourceFormat = "json",
            TargetFormat = "xml",
            Mappings =
            [
                new FieldMapping
                {
                    SourcePath = "name",
                    TargetPath = "name",
                    Required = true
                }
            ]
        };

        var exception = Record.Exception(() =>
            engine.Execute(
                input,
                output,
                template));

        Assert.Null(exception);

        Assert.NotEmpty(output.ToArray());
    }

    private static WarpEngine CreateEngine(
        IAuditSink auditSink)
    {
        var parsers = new ParserRegistry(
        [
            new JsonParser(),
            new XmlParser(),
            new CsvParser()
        ]);

        var serializers = new SerializerRegistry(
        [
            new XmlCanonicalSerializer()
        ]);

        return new WarpEngine(
            parsers,
            serializers,
            new Transformer(),
            auditSink);
    }

    private sealed class RecordingAuditSink : IAuditSink
    {
        public List<WarpAuditEvent> Events { get; } = [];

        public void Record(WarpAuditEvent auditEvent)
        {
            Events.Add(auditEvent);
        }
    }

    private sealed class ThrowingAuditSink : IAuditSink
    {
        public void Record(WarpAuditEvent auditEvent)
        {
            throw new InvalidOperationException(
                "Falha simulada no sink.");
        }
    }
}