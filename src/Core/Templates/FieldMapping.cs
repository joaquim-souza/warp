namespace Warp.Core.Templates;

/// <summary>
/// Um mapeamento de campo dentro de um <see cref="TemplateDefinition"/>.
/// SourcePath usa notação com ponto (ver <see cref="Warp.Core.Model.CanonicalNode.Navigate"/>).
/// </summary>
public sealed class FieldMapping
{
    public string SourcePath { get; set; } = "";
    public string TargetPath { get; set; } = "";
    public bool Required { get; set; } = false;
    public string? DefaultValue { get; set; }
    public TransformType Transform { get; set; } = TransformType.None;
}