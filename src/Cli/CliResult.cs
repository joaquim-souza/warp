namespace Warp.Cli;

public static class CliResult
{
    public const int Success = 0;
    public const int InvalidArguments = 2;
    public const int InputNotFound = 3;
    public const int TemplateError = 4;
    public const int ConversionError = 5;
}