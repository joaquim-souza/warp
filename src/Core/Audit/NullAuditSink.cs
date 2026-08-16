namespace Warp.Core.Audit;

/// <summary>
/// Sink padrão que descarta eventos.
///
/// Útil quando o consumidor não precisa de auditoria e também evita
/// que o Engine precise conhecer infraestrutura externa.
/// </summary>
public sealed class NullAuditSink : IAuditSink
{
    public void Record(WarpAuditEvent auditEvent)
    {
        ArgumentNullException.ThrowIfNull(auditEvent);
    }
}