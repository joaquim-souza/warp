namespace Warp.Core.Templates;

public sealed class TemplateDefinition
{
    public string Id { get; set; } = string.Empty;

    public int Version { get; set; }

    public string SourceFormat { get; set; } = string.Empty;

    public string TargetFormat { get; set; } = string.Empty;

    public string? Extends { get; set; }

    public string? RecordsPath { get; set; }

    public string? OutputRoot { get; set; }

    public string Encoding { get; set; } = "utf-8";

    public List<FieldMapping> Mappings { get; set; } = [];

    public List<CollectionMapping> Collections { get; set; } = [];
}