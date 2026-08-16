using System.Text;
using Warp.Core.Engine;
using Warp.Core.Parsing;
using Warp.Core.Registry;
using Warp.Core.Serialization;
using Warp.Core.Templates;
using Warp.Core.Transform;
using Xunit;

namespace Tests.Integration;

public sealed class GoldenFileTests
{
    [Fact]
    public void JsonToXml_ShouldMatchGoldenFile()
    {
        var inputPath =
            FindRepositoryFile(
                "samples/json/product.json");

        var templatePath =
            FindRepositoryFile(
                "templates/json-to-xml.v1.yaml");

        var goldenPath =
            FindRepositoryFile(
                "tests/GoldenFiles/json-to-xml.xml");

        var parsers =
            new ParserRegistry(
            [
                new JsonParser(),
                new CsvParser(),
                new XmlParser()
            ]);

        var serializers =
            new SerializerRegistry(
            [
                new XmlCanonicalSerializer()
            ]);

        var transformer =
            new Transformer();

        var engine =
            new WarpEngine(
                parsers,
                serializers,
                transformer);

        var loader =
            new TemplateLoader();

        var template =
            loader.Load(templatePath);

        using var input =
            File.OpenRead(inputPath);

        using var output =
            new MemoryStream();

        var result =
            engine.Execute(
                input,
                output,
                template);

        Assert.True(
            result.IsSuccess,
            BuildFailureMessage(result));

        var actual =
            output.ToArray();

        var expected =
            File.ReadAllBytes(
                goldenPath);

        Assert.Equal(
            expected,
            actual);
    }

    private static string FindRepositoryFile(
        string relativePath)
    {
        var directory =
            new DirectoryInfo(
                AppContext.BaseDirectory);

        while (directory is not null)
        {
            var candidate =
                Path.Combine(
                    directory.FullName,
                    relativePath);

            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory =
                directory.Parent;
        }

        throw new FileNotFoundException(
            $"Arquivo do repositório não encontrado: {relativePath}");
    }

    private static string BuildFailureMessage(
        WarpResult result)
    {
        if (result.Validation.Errors.Count == 0)
        {
            return "A execução do WARP falhou sem erros de validação.";
        }

        return string.Join(
            Environment.NewLine,
            result.Validation.Errors.Select(
                error =>
                    $"[{error.Path}] {error.Message}"));
    }
}