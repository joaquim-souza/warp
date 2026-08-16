using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Warp.Core.Templates;

public sealed class TemplateWriter
{
    private readonly ISerializer _serializer;

    public TemplateWriter()
    {
        _serializer =
            new SerializerBuilder()
                .WithNamingConvention(
                    CamelCaseNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(
                    DefaultValuesHandling.OmitNull)
                .Build();
    }

    public void Write(
        TemplateDefinition template,
        string path)
    {
        if (template is null)
            throw new ArgumentNullException(
                nameof(template));

        var directory =
            Path.GetDirectoryName(
                Path.GetFullPath(path));

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var yaml =
            _serializer.Serialize(template);

        File.WriteAllText(
            path,
            yaml);
    }
}