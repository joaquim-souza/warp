using Warp.Core.Templates;

namespace Tests.Template;

public sealed class TemplatePathValidatorTests
{
    [Theory]
    [InlineData("product")]
    [InlineData("product.id")]
    [InlineData("product.identification.code")]
    [InlineData("_product.id")]
    [InlineData("product-item.code")]
    public void IsValid_ShouldAcceptValidPaths(string path)
    {
        Assert.True(
            TemplatePathValidator.IsValid(path));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(".")]
    [InlineData(".product")]
    [InlineData("product.")]
    [InlineData("product..id")]
    [InlineData("123product")]
    [InlineData("product id")]
    public void IsValid_ShouldRejectInvalidPaths(string path)
    {
        Assert.False(
            TemplatePathValidator.IsValid(path));
    }
}