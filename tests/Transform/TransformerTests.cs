using Warp.Core.Model;
using Warp.Core.Templates;
using Warp.Core.Transform;

namespace Tests.Transform;

public sealed class TransformerTests
{
    [Fact]
    public void Transform_ShouldMapSingleDocument()
    {
        var root = new CanonicalNode("root");
        var product = root.AddChild("product");

        product.AddChild("id", "123");
        product.AddChild("name", "Keyboard");

        var source = new CanonicalDocument(root, "json");

        var template = new TemplateDefinition
        {
            Id = "test",
            Version = 1,
            SourceFormat = "json",
            TargetFormat = "xml",
            Mappings =
            [
                new FieldMapping
                {
                    SourcePath = "product.id",
                    TargetPath = "product.id",
                    Required = true
                },
                new FieldMapping
                {
                    SourcePath = "product.name",
                    TargetPath = "product.name",
                    Required = true
                }
            ]
        };

        var (output, validation) =
            new Transformer().Transform(source, template);

        Assert.True(validation.IsValid);

        Assert.Equal(
            "123",
            output.Root.Navigate("product.id")?.Value);

        Assert.Equal(
            "Keyboard",
            output.Root.Navigate("product.name")?.Value);
    }

    [Fact]
    public void Transform_ShouldFailWhenRequiredFieldIsMissing()
    {
        var root = new CanonicalNode("root");
        var source = new CanonicalDocument(root, "json");

        var template = new TemplateDefinition
        {
            Id = "test",
            Version = 1,
            SourceFormat = "json",
            TargetFormat = "xml",
            Mappings =
            [
                new FieldMapping
                {
                    SourcePath = "product.id",
                    TargetPath = "product.id",
                    Required = true
                }
            ]
        };

        var (_, validation) =
            new Transformer().Transform(source, template);

        Assert.False(validation.IsValid);
        Assert.Single(validation.Errors);
    }

    [Fact]
    public void Transform_ShouldApplyDefaultValue()
    {
        var root = new CanonicalNode("root");

        var source = new CanonicalDocument(root, "json");

        var template = new TemplateDefinition
        {
            Id = "test",
            Version = 1,
            SourceFormat = "json",
            TargetFormat = "xml",
            Mappings =
            [
                new FieldMapping
                {
                    SourcePath = "product.category",
                    TargetPath = "product.category",
                    DefaultValue = "unknown"
                }
            ]
        };

        var (output, validation) =
            new Transformer().Transform(source, template);

        Assert.True(validation.IsValid);

        Assert.Equal(
            "unknown",
            output.Root.Navigate("product.category")?.Value);
    }

    [Fact]
    public void Transform_ShouldApplyUpperTransform()
    {
        var root = new CanonicalNode("root");
        root.AddChild("name", "keyboard");

        var source = new CanonicalDocument(root, "json");

        var template = new TemplateDefinition
        {
            Id = "test",
            Version = 1,
            SourceFormat = "json",
            TargetFormat = "xml",
            Mappings =
            [
                new FieldMapping
                {
                    SourcePath = "name",
                    TargetPath = "name",
                    Transform = TransformType.Upper
                }
            ]
        };

        var (output, validation) =
            new Transformer().Transform(source, template);

        Assert.True(validation.IsValid);

        Assert.Equal(
            "KEYBOARD",
            output.Root.Child("name")?.Value);
    }

    [Fact]
    public void Transform_ShouldTransformMultipleCsvRecords()
    {
        var root = new CanonicalNode("rows");

        var row1 = root.AddChild("row");
        row1.AddChild("sku", "001");

        var row2 = root.AddChild("row");
        row2.AddChild("sku", "002");

        var source = new CanonicalDocument(root, "csv");

        var template = new TemplateDefinition
        {
            Id = "csv-to-xlsx",
            Version = 1,
            SourceFormat = "csv",
            TargetFormat = "xlsx",
            RecordsPath = "row",
            Mappings =
            [
                new FieldMapping
                {
                    SourcePath = "sku",
                    TargetPath = "sku",
                    Required = true
                }
            ]
        };

        var (output, validation) =
            new Transformer().Transform(source, template);

        Assert.True(validation.IsValid);

        Assert.Equal("records", output.Root.Name);

        var records = output.Root
            .ChildrenNamed("record")
            .ToList();

        Assert.Equal(2, records.Count);
        Assert.Equal("001", records[0].Child("sku")?.Value);
        Assert.Equal("002", records[1].Child("sku")?.Value);
    }

    [Fact]
    public void Transform_ShouldBeDeterministic()
    {
        var root = new CanonicalNode("root");
        root.AddChild("name", "Keyboard");
        root.AddChild("price", "99.90");

        var source = new CanonicalDocument(root, "json");

        var template = new TemplateDefinition
        {
            Id = "test",
            Version = 1,
            SourceFormat = "json",
            TargetFormat = "xml",
            Mappings =
            [
                new FieldMapping
                {
                    SourcePath = "name",
                    TargetPath = "product.name"
                },
                new FieldMapping
                {
                    SourcePath = "price",
                    TargetPath = "product.price",
                    Transform = TransformType.ToNumber
                }
            ]
        };

        var transformer = new Transformer();

        var result1 = transformer.Transform(source, template);
        var result2 = transformer.Transform(source, template);

        Assert.True(result1.Validation.IsValid);
        Assert.True(result2.Validation.IsValid);

        Assert.Equal(
            result1.Output.Root.Navigate("product.name")?.Value,
            result2.Output.Root.Navigate("product.name")?.Value);

        Assert.Equal(
            result1.Output.Root.Navigate("product.price")?.Value,
            result2.Output.Root.Navigate("product.price")?.Value);
    }
}