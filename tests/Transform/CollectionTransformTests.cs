using Warp.Core.Model;
using Warp.Core.Templates;
using Warp.Core.Transform;

namespace Tests.Transform;

public sealed class CollectionTransformTests
{
    [Fact]
    public void Transform_ShouldMapCollection()
    {
        var root = new CanonicalNode("order");

        root.AddChild("id", "123");

        var items = root.AddChild("items");

        var item1 = items.AddChild("item");
        item1.AddChild("sku", "A01");
        item1.AddChild("name", "Keyboard");
        item1.AddChild("quantity", "2");

        var item2 = items.AddChild("item");
        item2.AddChild("sku", "B02");
        item2.AddChild("name", "Mouse");
        item2.AddChild("quantity", "5");

        var source =
            new CanonicalDocument(
                root,
                "json");

        var template =
            new TemplateDefinition
            {
                Id = "collection-test",
                Version = 1,
                SourceFormat = "json",
                TargetFormat = "xml",

                Collections =
                [
                    new CollectionMapping
                    {
                        SourcePath = "items",
                        TargetPath = "items.item",

                        Mappings =
                        [
                            new FieldMapping
                            {
                                SourcePath = "sku",
                                TargetPath = "sku",
                                Required = true
                            },

                            new FieldMapping
                            {
                                SourcePath = "name",
                                TargetPath = "name",
                                Required = true
                            },

                            new FieldMapping
                            {
                                SourcePath = "quantity",
                                TargetPath = "quantity",
                                Required = true
                            }
                        ]
                    }
                ]
            };

        var transformer =
            new Transformer();

        var (output, validation) =
            transformer.Transform(
                source,
                template);

        Assert.True(
            validation.IsValid);

        var outputItems =
            output.Root
                .Child("items")?
                .ChildrenNamed("item")
                .ToList();

        Assert.NotNull(outputItems);
        Assert.Equal(2, outputItems!.Count);

        Assert.Equal(
            "A01",
            outputItems[0].Child("sku")?.Value);

        Assert.Equal(
            "Keyboard",
            outputItems[0].Child("name")?.Value);

        Assert.Equal(
            "2",
            outputItems[0].Child("quantity")?.Value);

        Assert.Equal(
            "B02",
            outputItems[1].Child("sku")?.Value);

        Assert.Equal(
            "Mouse",
            outputItems[1].Child("name")?.Value);

        Assert.Equal(
            "5",
            outputItems[1].Child("quantity")?.Value);
    }

    [Fact]
    public void Transform_ShouldPreserveCollectionOrder()
    {
        var root =
            new CanonicalNode("root");

        var items =
            root.AddChild("items");

        var first =
            items.AddChild("item");

        first.AddChild("sku", "001");

        var second =
            items.AddChild("item");

        second.AddChild("sku", "002");

        var third =
            items.AddChild("item");

        third.AddChild("sku", "003");

        var source =
            new CanonicalDocument(
                root,
                "json");

        var template =
            new TemplateDefinition
            {
                Id = "order-test",
                Version = 1,
                SourceFormat = "json",
                TargetFormat = "xml",

                Collections =
                [
                    new CollectionMapping
                    {
                        SourcePath = "items",
                        TargetPath = "items.item",

                        Mappings =
                        [
                            new FieldMapping
                            {
                                SourcePath = "sku",
                                TargetPath = "sku",
                                Required = true
                            }
                        ]
                    }
                ]
            };

        var (output, validation) =
            new Transformer()
                .Transform(
                    source,
                    template);

        Assert.True(
            validation.IsValid);

        var outputItems =
            output.Root
                .Child("items")?
                .ChildrenNamed("item")
                .ToList();

        Assert.NotNull(outputItems);
        Assert.Equal(3, outputItems!.Count);

        Assert.Equal(
            "001",
            outputItems[0].Child("sku")?.Value);

        Assert.Equal(
            "002",
            outputItems[1].Child("sku")?.Value);

        Assert.Equal(
            "003",
            outputItems[2].Child("sku")?.Value);
    }
}