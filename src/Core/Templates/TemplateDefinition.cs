using YamlDotNet.Serialization;

namespace Warp.Core.Templates;

public sealed class TemplateDefinition
{
    [YamlMember(Alias = "Id")]
    public string Id { get; set; } = string.Empty;

    [YamlMember(Alias = "Version")]
    public int Version { get; set; }

    [YamlMember(Alias = "SourceFormat")]
    public string SourceFormat { get; set; } = string.Empty;

    [YamlMember(Alias = "TargetFormat")]
    public string TargetFormat { get; set; } = string.Empty;

    [YamlMember(Alias = "Description")]
    public string? Description { get; set; }

    [YamlMember(Alias = "Extends")]
    public string? Extends { get; set; }

    [YamlMember(Alias = "RecordsPath")]
    public string? RecordsPath { get; set; }

    [YamlMember(Alias = "OutputRoot")]
    public string? OutputRoot { get; set; }

    [YamlMember(Alias = "Mappings")]
    public List<FieldMapping> Mappings { get; set; } = [];

    [YamlMember(Alias = "Collections")]
    public List<CollectionMapping> Collections { get; set; } = [];
}