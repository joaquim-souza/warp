namespace Warp.Core.Audit;

/// <summary>
/// Evento imutável produzido por uma execução do WARP.
/// </summary>
public sealed record WarpAuditEvent(
    string TemplateId,
    int TemplateVersion,
    string SourceFormat,
    string TargetFormat,
    bool Success,
    int ValidationErrorCount,
    TimeSpan Duration);