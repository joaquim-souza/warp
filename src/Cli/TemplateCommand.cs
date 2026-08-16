using Warp.Core.Templates;

namespace Warp.Cli;

public static class TemplateCommand
{
    public static int Create(
        string[] args)
    {
        if (args.Length < 4)
        {
            PrintUsage();
            return 1;
        }

        var id =
            args[0];

        var source =
            args[1];

        var target =
            args[2];

        var output =
            args[3];

        var builder =
            new TemplateBuilder(id)
                .From(source)
                .To(target)
                .OutputRoot(
                    Path.GetFileNameWithoutExtension(
                        output));

        var template =
            builder.Build();

        var writer =
            new TemplateWriter();

        writer.Write(
            template,
            output);

        Console.WriteLine(
            $"Template criado: {output}");

        return 0;
    }

    private static void PrintUsage()
    {
        Console.WriteLine(
            "Uso: warp template create " +
            "<id> <source> <target> <output>");
    }

    public static int Validate(string[] args)
{
    if (args.Length != 1)
    {
        Console.Error.WriteLine(
            "Uso: warp template validate <template>");

        return 2;
    }

    var path = args[0];

    try
    {
        var loader = new TemplateLoader();

        var template =
            loader.Load(path);

        Console.WriteLine(
            "Template válido.");

        Console.WriteLine(
            $"ID: {template.Id}");

        Console.WriteLine(
            $"Version: {template.Version}");

        Console.WriteLine(
            $"Source: {template.SourceFormat}");

        Console.WriteLine(
            $"Target: {template.TargetFormat}");

        Console.WriteLine(
            $"Mappings: {template.Mappings.Count}");

        Console.WriteLine(
            $"Collections: {template.Collections.Count}");

        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(
            "Template inválido.");

        Console.Error.WriteLine(
            ex.Message);

        return 1;
    }
}
}