using Warp.Core.Templates;

namespace Tests.Template;

public sealed class TemplateBuilderTests
{
    [Fact]
    public void Build_ShouldCreateBasicTemplate()
    {
        var template =
            new TemplateBuilder(
                "csv-products")
            .From("csv")
            .To("xml")
            .OutputRoot("products")
            .Build();

        Assert.Equal(
            "csv-products",
            template.Id);

        Assert.Equal(
            1,
            template.Version);

        Assert.Equal(
            "csv",
            template.SourceFormat);

        Assert.Equal(
            "xml",
            template.TargetFormat);

        Assert.Equal(
            "products",
            template.OutputRoot);
    }

    [Fact]
    public void Build_ShouldCreateMapping()
    {
        var template =
            new TemplateBuilder(
                "product")
            .From("json")
            .To("xml")
            .Map(
                "product.id",
                "product.id",
                required: true)
            .Build();

        Assert.Single(
            template.Mappings);

        var mapping =
            template.Mappings[0];

        Assert.Equal(
            "product.id",
            mapping.SourcePath);

        Assert.Equal(
            "product.id",
            mapping.TargetPath);

        Assert.True(
            mapping.Required);
    }

    [Fact]
    public void Build_ShouldSupportInheritance()
    {
        var template =
            new TemplateBuilder(
                "product-xml")
            .Extends(
                "base/product-base.v1.yaml")
            .From("json")
            .To("xml")
            .Build();

        Assert.Equal(
            "base/product-base.v1.yaml",
            template.Extends);

        Assert.Equal(
            "json",
            template.SourceFormat);

        Assert.Equal(
            "xml",
            template.TargetFormat);
    }

    [Fact]
    public void Build_ShouldCreateIndependentTemplate()
    {
        var builder =
            new TemplateBuilder(
                "products")
            .From("csv")
            .To("xml")
            .Map(
                "sku",
                "product.sku");

        var template1 =
            builder.Build();

        var template2 =
            builder.Build();

        Assert.NotSame(
            template1,
            template2);

        Assert.Single(
            template1.Mappings);

        Assert.Single(
            template2.Mappings);
    }
}