namespace Warp.Core.Templates;

public sealed class FieldMapping
{
    public string SourcePath { get; set; } = "";

    public string TargetPath { get; set; } = "";

    public bool Required { get; set; }

    public string? DefaultValue { get; set; }

    public TransformType Transform { get; set; }
        = TransformType.None;
}