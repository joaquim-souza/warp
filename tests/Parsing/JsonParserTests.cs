using System.Text;
using Warp.Core.Parsing;

namespace Tests.Parsing;

public sealed class JsonParserTests
{
    [Fact]
    public void Parse_ShouldCreateCanonicalTree()
    {
        const string json = """
        {
            "product": {
                "id": "123",
                "name": "Keyboard",
                "price": 99.90
            }
        }
        """;

        using var stream = new MemoryStream(
            Encoding.UTF8.GetBytes(json));

        var document = new JsonParser().Parse(stream);

        Assert.Equal("json", document.SourceFormat);
        Assert.Equal("root", document.Root.Name);

        Assert.Equal(
            "123",
            document.Root.Navigate("product.id")?.Value);

        Assert.Equal(
            "Keyboard",
            document.Root.Navigate("product.name")?.Value);

        Assert.Equal(
            "99.90",
            document.Root.Navigate("product.price")?.Value);
    }

    [Fact]
    public void Parse_ShouldRepresentArraysAsItems()
    {
        const string json = """
        {
            "products": [
                { "id": "1" },
                { "id": "2" }
            ]
        }
        """;

        using var stream = new MemoryStream(
            Encoding.UTF8.GetBytes(json));

        var document = new JsonParser().Parse(stream);

        var products = document.Root.Child("products");

        Assert.NotNull(products);

        var items = products!
            .ChildrenNamed("item")
            .ToList();

        Assert.Equal(2, items.Count);
        Assert.Equal("1", items[0].Child("id")?.Value);
        Assert.Equal("2", items[1].Child("id")?.Value);
    }
}