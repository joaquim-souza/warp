using Warp.Cli;

namespace Tests.Integration;

public sealed class CliTests
{
    [Fact]
    public void Parse_ShouldReadAllRequiredArguments()
    {
        var options = CliOptions.Parse(
        [
            "--input", "input.csv",
            "--output", "output.xlsx",
            "--template", "template.yaml"
        ]);

        Assert.Equal("input.csv", options.Input);
        Assert.Equal("output.xlsx", options.Output);
        Assert.Equal("template.yaml", options.Template);
        Assert.False(options.Force);
    }

    [Fact]
    public void Parse_ShouldAcceptShortArguments()
    {
        var options = CliOptions.Parse(
        [
            "-i", "input.csv",
            "-o", "output.xlsx",
            "-t", "template.yaml",
            "-f"
        ]);

        Assert.Equal("input.csv", options.Input);
        Assert.Equal("output.xlsx", options.Output);
        Assert.Equal("template.yaml", options.Template);
        Assert.True(options.Force);
    }

    [Fact]
    public void Parse_ShouldAcceptHelp()
    {
        var options = CliOptions.Parse(["--help"]);

        Assert.True(options.ShowHelp);
    }

    [Fact]
    public void Parse_ShouldRejectMissingInput()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CliOptions.Parse(
            [
                "--output", "output.xlsx",
                "--template", "template.yaml"
            ]));

        Assert.Contains("--input", exception.Message);
    }

    [Fact]
    public void Parse_ShouldRejectMissingOutput()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CliOptions.Parse(
            [
                "--input", "input.csv",
                "--template", "template.yaml"
            ]));

        Assert.Contains("--output", exception.Message);
    }

    [Fact]
    public void Parse_ShouldRejectMissingTemplate()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CliOptions.Parse(
            [
                "--input", "input.csv",
                "--output", "output.xlsx"
            ]));

        Assert.Contains("--template", exception.Message);
    }

    [Fact]
    public void Parse_ShouldRejectUnknownArgument()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CliOptions.Parse(
            [
                "--input", "input.csv",
                "--output", "output.xlsx",
                "--template", "template.yaml",
                "--banana"
            ]));

        Assert.Contains("--banana", exception.Message);
    }

    [Fact]
    public void Parse_ShouldRejectArgumentWithoutValue()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CliOptions.Parse(
            [
                "--input",
                "--output", "output.xlsx",
                "--template", "template.yaml"
            ]));

        Assert.Contains("--input", exception.Message);
    }
}