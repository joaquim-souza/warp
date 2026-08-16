namespace Warp.Core.Audit;

/// <summary>
/// Destino abstrato para eventos de auditoria do WARP.
///
/// O Core não sabe se o evento será gravado em arquivo, banco, Kafka,
/// OpenTelemetry ou simplesmente descartado.
/// </summary>
public interface IAuditSink
{
    void Record(WarpAuditEvent auditEvent);
}