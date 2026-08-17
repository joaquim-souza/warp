using System.Text;
using Warp.Core.Engine;
using Warp.Core.Parsing;
using Warp.Core.Registry;
using Warp.Core.Serialization;
using Warp.Core.Templates;
using Warp.Core.Transform;
using Warp.Excel;

namespace Tests.Golden;

public sealed class GoldenFileTests
{
    [Fact]
    public void JsonToXml_ShouldProduceDeterministicGoldenFile()
    {
        var baseDirectory =
            AppContext.BaseDirectory;

        var fixtureDirectory =
            Path.Combine(
                baseDirectory,
                "Golden",
                "Fixtures");

        var expectedDirectory =
            Path.Combine(
                baseDirectory,
                "Golden",
                "Expected");

        var inputPath =
            Path.Combine(
                fixtureDirectory,
                "products.json");

        var templatePath =
            Path.Combine(
                fixtureDirectory,
                "json-to-xml.v1.yaml");

        var expectedPath =
            Path.Combine(
                expectedDirectory,
                "products.xml");

        Assert.True(
            File.Exists(inputPath),
            $"Fixture não encontrado: {inputPath}");

        Assert.True(
            File.Exists(templatePath),
            $"Template não encontrado: {templatePath}");

        Assert.True(
            File.Exists(expectedPath),
            $"Golden file não encontrado: {expectedPath}");

        var parsers =
            new ParserRegistry(
            [
                new CsvParser(),
                new JsonParser(),
                new XmlParser()
            ]);

        var serializers =
            new SerializerRegistry(
            [
                new XmlCanonicalSerializer(),
                new ExcelSerializer()
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
            loader.Load(
                templatePath);

        byte[] actualBytes;

        using (var input =
               File.OpenRead(inputPath))
        using (var output =
               new MemoryStream())
        {
            var result =
                engine.Execute(
                    input,
                    output,
                    template);

            Assert.True(
                result.IsSuccess,
                string.Join(
                    Environment.NewLine,
                    result.Validation.Errors.Select(
                        error =>
                            $"[{error.Path}] {error.Message}")));

            actualBytes =
                output.ToArray();
        }

        var expectedBytes =
            File.ReadAllBytes(
                expectedPath);

        Assert.Equal(
            expectedBytes,
            actualBytes);
    }
}