using Warp.Core.Templates;

namespace Tests.Template;

public sealed class TemplateWriterTests
{
    [Fact]
    public void Write_ShouldCreateYamlFile()
    {
        var directory =
            Path.Combine(
                Path.GetTempPath(),
                "warp-tests",
                Guid.NewGuid().ToString());

        var path =
            Path.Combine(
                directory,
                "product.v1.yaml");

        var template =
            new TemplateBuilder(
                "product")
            .From("json")
            .To("xml")
            .OutputRoot("products")
            .Map(
                "product.id",
                "product.id",
                required: true)
            .Build();

        new TemplateWriter()
            .Write(
                template,
                path);

        Assert.True(
            File.Exists(path));

        var content =
            File.ReadAllText(path);

        Assert.Contains(
            "id: product",
            content);

        Assert.Contains(
            "sourceFormat: json",
            content);

        Assert.Contains(
            "targetFormat: xml",
            content);

        Assert.Contains(
            "outputRoot: products",
            content);

        Assert.Contains(
            "sourcePath: product.id",
            content);
    }
}