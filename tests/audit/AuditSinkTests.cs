using Warp.Core.Audit;

namespace Tests.Audit;

public sealed class AuditSinkTests
{
    [Fact]
    public void NullAuditSink_ShouldAcceptEvent()
    {
        var sink = new NullAuditSink();

        var auditEvent = new WarpAuditEvent(
            "test",
            1,
            "json",
            "xml",
            true,
            0,
            TimeSpan.FromMilliseconds(10));

        var exception = Record.Exception(() =>
            sink.Record(auditEvent));

        Assert.Null(exception);
    }

    [Fact]
    public void AuditEvent_ShouldPreserveExecutionInformation()
    {
        var duration = TimeSpan.FromMilliseconds(42);

        var auditEvent = new WarpAuditEvent(
            "csv-to-xlsx",
            1,
            "csv",
            "xlsx",
            true,
            0,
            duration);

        Assert.Equal("csv-to-xlsx", auditEvent.TemplateId);
        Assert.Equal(1, auditEvent.TemplateVersion);
        Assert.Equal("csv", auditEvent.SourceFormat);
        Assert.Equal("xlsx", auditEvent.TargetFormat);
        Assert.True(auditEvent.Success);
        Assert.Equal(0, auditEvent.ValidationErrorCount);
        Assert.Equal(duration, auditEvent.Duration);
    }
}