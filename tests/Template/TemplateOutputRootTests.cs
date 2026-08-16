using Warp.Core.Templates;

namespace Tests.Template;

public sealed class TemplateOutputRootTests
{
    [Fact]
    public void Validator_ShouldAcceptValidOutputRoot()
    {
        var template = new TemplateDefinition
        {
            Id = "test",
            Version = 1,
            SourceFormat = "json",
            TargetFormat = "xml",
            OutputRoot = "Product",
            Mappings =
            [
                new FieldMapping
                {
                    SourcePath = "id",
                    TargetPath = "id"
                }
            ]
        };

        var result = new TemplateValidator()
            .Validate(template);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validator_ShouldRejectOutputRootWithPath()
    {
        var template = new TemplateDefinition
        {
            Id = "test",
            Version = 1,
            SourceFormat = "json",
            TargetFormat = "xml",
            OutputRoot = "product.item",
            Mappings =
            [
                new FieldMapping
                {
                    SourcePath = "id",
                    TargetPath = "id"
                }
            ]
        };

        var result = new TemplateValidator()
            .Validate(template);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.Path == "OutputRoot");
    }

    [Fact]
    public void Validator_ShouldRejectOutputRootWithWhitespace()
    {
        var template = new TemplateDefinition
        {
            Id = "test",
            Version = 1,
            SourceFormat = "json",
            TargetFormat = "xml",
            OutputRoot = "Product Root",
            Mappings =
            [
                new FieldMapping
                {
                    SourcePath = "id",
                    TargetPath = "id"
                }
            ]
        };

        var result = new TemplateValidator()
            .Validate(template);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.Path == "OutputRoot");
    }
}