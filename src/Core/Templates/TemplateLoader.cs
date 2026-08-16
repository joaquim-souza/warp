using YamlDotNet.Serialization;

namespace Warp.Core.Templates;

public sealed class TemplateLoader
{
    private readonly IDeserializer _deserializer;
    private readonly TemplateValidator _validator;
    private readonly TemplateComposer _composer;

    public TemplateLoader()
    {
        _deserializer =
            new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();

        _validator = new TemplateValidator();
        _composer = new TemplateComposer();
    }

    public TemplateDefinition Load(string path)
    {
        return LoadInternal(
            path,
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase));
    }

    private TemplateDefinition LoadInternal(
        string path,
        HashSet<string> loading)
    {
        var fullPath = Path.GetFullPath(path);

        if (!loading.Add(fullPath))
        {
            throw new InvalidOperationException(
                $"Ciclo de herança detectado no template: '{fullPath}'.");
        }

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException(
                $"Template não encontrado: '{fullPath}'.",
                fullPath);
        }

        var yaml = File.ReadAllText(fullPath);

        var template =
            _deserializer.Deserialize<TemplateDefinition>(yaml);

        if (template is null)
        {
            throw new InvalidOperationException(
                $"Template vazio ou inválido: '{fullPath}'.");
        }

        if (!string.IsNullOrWhiteSpace(template.Extends))
        {
            var basePath =
                ResolveBaseTemplate(
                    fullPath,
                    template.Extends);

            var baseTemplate =
                LoadInternal(
                    basePath,
                    loading);

            template =
                _composer.Compose(
                    baseTemplate,
                    template);
        }

        loading.Remove(fullPath);

        _validator.Validate(template);

        return template;
    }

    private static string ResolveBaseTemplate(
        string childPath,
        string extends)
    {
        var directory =
            Path.GetDirectoryName(childPath)
            ?? Directory.GetCurrentDirectory();

        var candidate =
            Path.Combine(
                directory,
                extends);

        if (File.Exists(candidate))
            return candidate;

        if (!Path.HasExtension(candidate))
        {
            var yamlCandidate = candidate + ".yaml";

            if (File.Exists(yamlCandidate))
                return yamlCandidate;

            var ymlCandidate = candidate + ".yml";

            if (File.Exists(ymlCandidate))
                return ymlCandidate;
        }

        throw new FileNotFoundException(
            $"Template base '{extends}' não encontrado. " +
            $"Procurado a partir de '{directory}'.");
    }
}