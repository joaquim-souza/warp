using Warp.Core.Templates;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Warp.Cli;

public sealed class TemplateBuilder
{
    private readonly IDeserializer _deserializer;
    private readonly ISerializer _serializer;

    public TemplateBuilder()
    {
        _deserializer =
            new DeserializerBuilder()
                .WithNamingConvention(
                    CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

        _serializer =
            new SerializerBuilder()
                .WithNamingConvention(
                    CamelCaseNamingConvention.Instance)
                .Build();
    }

    public void AddMapping(
        string templatePath,
        string sourcePath,
        string targetPath,
        bool required = false,
        string? defaultValue = null,
        string? transform = null)
    {
        var template = Load(templatePath);

        var mapping = new FieldMapping
        {
            SourcePath = sourcePath,
            TargetPath = targetPath,
            Required = required,
            DefaultValue = defaultValue
        };

        if (!string.IsNullOrWhiteSpace(transform))
        {
            if (!Enum.TryParse<TransformType>(
                transform,
                true,
                out var transformType))
            {
                throw new ArgumentException(
                    $"Transformação inválida: '{transform}'.");
            }

            mapping.Transform = transformType;
        }

        template.Mappings.Add(mapping);

        Save(templatePath, template);
    }

    public void AddCollection(
        string templatePath,
        string sourcePath,
        string targetPath,
        bool required = true)
    {
        var template = Load(templatePath);

        if (template.Collections.Any(c =>
                c.SourcePath.Equals(
                    sourcePath,
                    StringComparison.OrdinalIgnoreCase) &&
                c.TargetPath.Equals(
                    targetPath,
                    StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException(
                $"A collection '{sourcePath} -> {targetPath}' já existe.");
        }

        template.Collections.Add(
            new CollectionMapping
            {
                SourcePath = sourcePath,
                TargetPath = targetPath,
                Required = required
            });

        Save(templatePath, template);
    }

    public void AddCollectionMapping(
        string templatePath,
        string collectionSourcePath,
        string sourcePath,
        string targetPath,
        bool required = false,
        string? defaultValue = null,
        string? transform = null)
    {
        var template = Load(templatePath);

        var collection =
            template.Collections.FirstOrDefault(c =>
                c.SourcePath.Equals(
                    collectionSourcePath,
                    StringComparison.OrdinalIgnoreCase));

        if (collection is null)
        {
            throw new ArgumentException(
                $"Collection '{collectionSourcePath}' não encontrada.");
        }

        var mapping = new FieldMapping
        {
            SourcePath = sourcePath,
            TargetPath = targetPath,
            Required = required,
            DefaultValue = defaultValue
        };

        if (!string.IsNullOrWhiteSpace(transform))
        {
            if (!Enum.TryParse<TransformType>(
                transform,
                true,
                out var transformType))
            {
                throw new ArgumentException(
                    $"Transformação inválida: '{transform}'.");
            }

            mapping.Transform = transformType;
        }

        collection.Mappings.Add(mapping);

        Save(templatePath, template);
    }

    private TemplateDefinition Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                $"Template não encontrado: '{path}'.",
                path);
        }

        var yaml = File.ReadAllText(path);

        var template =
            _deserializer.Deserialize<TemplateDefinition>(
                yaml);

        if (template is null)
        {
            throw new InvalidOperationException(
                $"Template inválido ou vazio: '{path}'.");
        }

        return template;
    }

    private void Save(
        string path,
        TemplateDefinition template)
    {
        var yaml =
            _serializer.Serialize(template);

        File.WriteAllText(
            path,
            yaml);
    }
}