using Warp.Core.Templates;

namespace Tests.Template;

public sealed class TemplateComposerTests
{
    [Fact]
    public void Compose_ShouldInheritBaseProperties()
    {
        var baseTemplate = new TemplateDefinition
        {
            Id = "base",
            Version = 1,
            SourceFormat = "json",
            TargetFormat = "xml",
            OutputRoot = "products",
            RecordsPath = "product"
        };

        var childTemplate = new TemplateDefinition
        {
            Id = "child",
            Version = 1,
            Extends = "base"
        };

        var result =
            new TemplateComposer()
                .Compose(
                    baseTemplate,
                    childTemplate);

        Assert.Equal(
            "json",
            result.SourceFormat);

        Assert.Equal(
            "xml",
            result.TargetFormat);

        Assert.Equal(
            "products",
            result.OutputRoot);

        Assert.Equal(
            "product",
            result.RecordsPath);
    }

    [Fact]
    public void Compose_ShouldOverrideBaseProperties()
    {
        var baseTemplate = new TemplateDefinition
        {
            Id = "base",
            Version = 1,
            SourceFormat = "json",
            TargetFormat = "xml",
            OutputRoot = "products"
        };

        var childTemplate = new TemplateDefinition
        {
            Id = "child",
            Version = 1,
            TargetFormat = "csv",
            OutputRoot = "items",
            Extends = "base"
        };

        var result =
            new TemplateComposer()
                .Compose(
                    baseTemplate,
                    childTemplate);

        Assert.Equal(
            "json",
            result.SourceFormat);

        Assert.Equal(
            "csv",
            result.TargetFormat);

        Assert.Equal(
            "items",
            result.OutputRoot);
    }

    [Fact]
    public void Compose_ShouldAppendMappings()
    {
        var baseTemplate = new TemplateDefinition
        {
            Id = "base",
            Version = 1,
            SourceFormat = "json",
            TargetFormat = "xml",
            Mappings =
            [
                new FieldMapping
                {
                    SourcePath = "product.id",
                    TargetPath = "product.id"
                }
            ]
        };

        var childTemplate = new TemplateDefinition
        {
            Id = "child",
            Version = 1,
            Extends = "base",
            Mappings =
            [
                new FieldMapping
                {
                    SourcePath = "product.name",
                    TargetPath = "product.name"
                }
            ]
        };

        var result =
            new TemplateComposer()
                .Compose(
                    baseTemplate,
                    childTemplate);

        Assert.Equal(
            2,
            result.Mappings.Count);

        Assert.Equal(
            "product.id",
            result.Mappings[0].SourcePath);

        Assert.Equal(
            "product.name",
            result.Mappings[1].SourcePath);
    }

    [Fact]
    public void Compose_ShouldNotMutateBaseTemplate()
    {
        var baseTemplate = new TemplateDefinition
        {
            Id = "base",
            Version = 1,
            SourceFormat = "json",
            TargetFormat = "xml",
            Mappings =
            [
                new FieldMapping
                {
                    SourcePath = "product.id",
                    TargetPath = "product.id"
                }
            ]
        };

        var childTemplate = new TemplateDefinition
        {
            Id = "child",
            Version = 1,
            Extends = "base",
            Mappings =
            [
                new FieldMapping
                {
                    SourcePath = "product.name",
                    TargetPath = "product.name"
                }
            ]
        };

        var result =
            new TemplateComposer()
                .Compose(
                    baseTemplate,
                    childTemplate);

        Assert.Single(
            baseTemplate.Mappings);

        Assert.Equal(
            2,
            result.Mappings.Count);
    }
}