using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Warp.Core.Templates;

/// <summary>
/// Carrega e valida um TemplateDefinition a partir de YAML.
/// </summary>
public sealed class TemplateLoader
{
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    private readonly TemplateValidator _validator;

    public TemplateLoader(TemplateValidator? validator = null)
    {
        _validator = validator ?? new TemplateValidator();
    }

    public TemplateDefinition Load(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        string yaml = File.ReadAllText(filePath);

        return LoadYaml(yaml);
    }

    public TemplateDefinition Load(Stream yamlStream)
    {
        ArgumentNullException.ThrowIfNull(yamlStream);

        using var reader = new StreamReader(yamlStream);

        return LoadYaml(reader.ReadToEnd());
    }

    private TemplateDefinition LoadYaml(string yamlContent)
    {
        if (string.IsNullOrWhiteSpace(yamlContent))
        {
            throw new InvalidOperationException(
                "Template YAML vazio ou inválido.");
        }

        var template = _deserializer.Deserialize<TemplateDefinition>(
            yamlContent);

        if (template is null)
        {
            throw new InvalidOperationException(
                "Template YAML vazio ou inválido.");
        }

        var validation = _validator.Validate(template);

        if (!validation.IsValid)
        {
            var details = string.Join(
                Environment.NewLine,
                validation.Errors.Select(error =>
                    $"- {error.Path}: {error.Message}"));

            throw new InvalidOperationException(
                $"Template inválido:{Environment.NewLine}{details}");
        }

        return template;
    }
}