namespace Warp.Core.Templates;

public sealed class TemplateBuilder
{
    private readonly TemplateDefinition _template;

    public TemplateBuilder(
        string id,
        int version = 1)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException(
                "Template ID não pode ser vazio.",
                nameof(id));

        _template = new TemplateDefinition
        {
            Id = id,
            Version = version
        };
    }

    public TemplateBuilder From(
        string sourceFormat)
    {
        _template.SourceFormat =
            sourceFormat;

        return this;
    }

    public TemplateBuilder To(
        string targetFormat)
    {
        _template.TargetFormat =
            targetFormat;

        return this;
    }

    public TemplateBuilder OutputRoot(
        string outputRoot)
    {
        _template.OutputRoot =
            outputRoot;

        return this;
    }

    public TemplateBuilder Records(
        string recordsPath)
    {
        _template.RecordsPath =
            recordsPath;

        return this;
    }

    public TemplateBuilder Extends(
        string template)
    {
        _template.Extends =
            template;

        return this;
    }

    public TemplateBuilder Map(
        string sourcePath,
        string targetPath,
        bool required = false,
        string? defaultValue = null,
        TransformType transform = TransformType.None)
    {
        _template.Mappings.Add(
            new FieldMapping
            {
                SourcePath = sourcePath,
                TargetPath = targetPath,
                Required = required,
                DefaultValue = defaultValue,
                Transform = transform
            });

        return this;
    }

    public TemplateBuilder Collection(
        CollectionMapping collection)
    {
        if (collection is null)
            throw new ArgumentNullException(
                nameof(collection));

        _template.Collections.Add(
            collection);

        return this;
    }

    public TemplateDefinition Build()
    {
        return new TemplateDefinition
        {
            Id = _template.Id,
            Version = _template.Version,
            SourceFormat = _template.SourceFormat,
            TargetFormat = _template.TargetFormat,
            Extends = _template.Extends,
            RecordsPath = _template.RecordsPath,
            OutputRoot = _template.OutputRoot,
            Mappings = [.. _template.Mappings],
            Collections = [.. _template.Collections]
        };
    }
}