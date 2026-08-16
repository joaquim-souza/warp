using Warp.Core.Model;

namespace Tests.Model;

public sealed class CanonicalNodeTests
{
    [Fact]
    public void AddChild_ShouldPreserveOrder()
    {
        var root = new CanonicalNode("root");

        root.AddChild("first", "1");
        root.AddChild("second", "2");
        root.AddChild("third", "3");

        Assert.Equal(
            ["first", "second", "third"],
            root.Children.Select(x => x.Name));
    }

    [Fact]
    public void Child_ShouldReturnFirstMatchingChild()
    {
        var root = new CanonicalNode("root");

        root.AddChild("item", "first");
        root.AddChild("item", "second");

        var result = root.Child("item");

        Assert.NotNull(result);
        Assert.Equal("first", result.Value);
    }

    [Fact]
    public void Navigate_ShouldFollowDottedPath()
    {
        var root = new CanonicalNode("root");

        var product = root.AddChild("product");
        product.AddChild("price", "10.50");

        var result = root.Navigate("product.price");

        Assert.NotNull(result);
        Assert.Equal("10.50", result.Value);
    }

    [Fact]
    public void Navigate_ShouldReturnNullForMissingPath()
    {
        var root = new CanonicalNode("root");

        var result = root.Navigate("product.price");

        Assert.Null(result);
    }
}