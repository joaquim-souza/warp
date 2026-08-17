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

    [Fact]
    public void Validate_ShouldRejectDuplicateTargetPaths()
    {
        var template = new TemplateDefinition
        {
            Id = "duplicate-target",
            Version = 1,
            SourceFormat = "json",
            TargetFormat = "xml",
            Mappings =
            [
                new FieldMapping
                {
                    SourcePath = "product.id",
                    TargetPath = "id"
                },
                new FieldMapping
                {
                    SourcePath = "product.code",
                    TargetPath = "id"
                }
            ]
        };

        var validator = new TemplateValidator();

        var result =
            validator.Validate(template);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.Path == "Mappings[1].TargetPath" &&
                error.Message.Contains(
                    "TargetPath duplicado"));
    }

    [Fact]
    public void Validate_ShouldRejectUndefinedTransform()
    {
        var template = new TemplateDefinition
        {
            Id = "invalid-transform",
            Version = 1,
            SourceFormat = "json",
            TargetFormat = "xml",
            Mappings =
            [
                new FieldMapping
                {
                    SourcePath = "product.price",
                    TargetPath = "price",
                    Transform = (TransformType)999
                }
            ]
        };

        var validator = new TemplateValidator();

        var result =
            validator.Validate(template);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.Path == "Mappings[0].Transform" &&
                error.Message.Contains(
                    "Transform inválido"));
    }

    [Fact]
    public void Validate_ShouldAcceptValidTransform()
    {
        var template = new TemplateDefinition
        {
            Id = "valid-transform",
            Version = 1,
            SourceFormat = "json",
            TargetFormat = "xml",
            Mappings =
            [
                new FieldMapping
                {
                    SourcePath = "product.price",
                    TargetPath = "price",
                    Transform = TransformType.ToNumber
                }
            ]
        };

        var validator = new TemplateValidator();

        var result =
            validator.Validate(template);

        Assert.True(result.IsValid);
    }
}