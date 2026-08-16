using Warp.Core.Templates;

namespace Tests.Templates;

public sealed class TemplateValidatorTests
{
    [Fact]
    public void Validate_ShouldAcceptValidTemplate()
    {
        var template = new TemplateDefinition
        {
            Id = "test",
            Version = 1,
            SourceFormat = "csv",
            TargetFormat = "xlsx",
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

        var result = new TemplateValidator().Validate(template);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldRejectMissingId()
    {
        var template = new TemplateDefinition
        {
            Version = 1,
            SourceFormat = "csv",
            TargetFormat = "xlsx",
            Mappings =
            [
                new FieldMapping
                {
                    SourcePath = "sku",
                    TargetPath = "sku"
                }
            ]
        };

        var result = new TemplateValidator().Validate(template);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.Path == "Id");
    }

    [Fact]
    public void Validate_ShouldRejectEmptyMappings()
    {
        var template = new TemplateDefinition
        {
            Id = "test",
            Version = 1,
            SourceFormat = "csv",
            TargetFormat = "xlsx"
        };

        var result = new TemplateValidator().Validate(template);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.Path == "Mappings");
    }
}