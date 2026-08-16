using Warp.Core.Templates;
using Warp.Core.Transform;
using YamlDotNet.Serialization;

namespace Warp.Cli;

public static class TemplateCommand
{
    public static int Create(string[] args)
    {
        try
        {
            var options = ParseCreateOptions(args);

            var template = new TemplateDefinition
            {
                Id = options.Id,
                Version = options.Version,
                SourceFormat = options.Source,
                TargetFormat = options.Target,
                OutputRoot = options.OutputRoot,
                Mappings = [],
                Collections = []
            };

            var serializer =
                new SerializerBuilder()
                    .Build();

            var yaml =
                serializer.Serialize(template);

            var directory =
                Path.GetDirectoryName(
                    Path.GetFullPath(options.Output));

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(
                options.Output,
                yaml);

            Console.WriteLine(
                $"Template criado: {options.Output}");

            return 0;
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(
                $"Erro de argumento: {ex.Message}");

            return 2;
        }
    }

    public static int Validate(string[] args)
    {
        try
        {
            if (args.Length != 1)
            {
                throw new ArgumentException(
                    "Uso: warp template validate <template>");
            }

            var loader =
                new TemplateLoader();

            var template =
                loader.Load(args[0]);

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
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(
                $"Erro de argumento: {ex.Message}");

            return 2;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"Template inválido: {ex.Message}");

            return 1;
        }
    }

    public static int AddMapping(string[] args)
    {
        try
        {
            var options =
                ParseMappingOptions(args);

            var builder =
                new TemplateBuilder();

            builder.AddMapping(
                options.Template,
                options.Source,
                options.Target,
                options.Required,
                options.DefaultValue,
                options.Transform == TransformType.None
        ? null
        : options.Transform.ToString());

            Console.WriteLine(
                $"Mapping adicionado: " +
                $"{options.Source} -> {options.Target}");

            return 0;
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(
                $"Erro de argumento: {ex.Message}");

            return 2;
        }
    }

    public static int AddCollection(string[] args)
    {
        try
        {
            var options =
                ParseCollectionOptions(args);

            var builder =
                new TemplateBuilder();

            builder.AddCollection(
                options.Template,
                options.Source,
                options.Target);

            Console.WriteLine(
                $"Collection adicionada: " +
                $"{options.Source} -> {options.Target}");

            return 0;
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(
                $"Erro de argumento: {ex.Message}");

            return 2;
        }
    }

    private static CreateOptions ParseCreateOptions(
        string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException(
                "Nenhum argumento informado.");
        }

        string? id = null;
        var version = 1;
        string? source = null;
        string? target = null;
        string? output = null;
        string? outputRoot = null;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--id":
                    id = ReadValue(
                        args,
                        ref i,
                        "--id");
                    break;

                case "--version":
                    var versionValue =
                        ReadValue(
                            args,
                            ref i,
                            "--version");

                    if (!int.TryParse(
                        versionValue,
                        out version))
                    {
                        throw new ArgumentException(
                            $"Versão inválida: '{versionValue}'.");
                    }

                    break;

                case "--source":
                    source = ReadValue(
                        args,
                        ref i,
                        "--source");
                    break;

                case "--target":
                    target = ReadValue(
                        args,
                        ref i,
                        "--target");
                    break;

                case "--output":
                    output = ReadValue(
                        args,
                        ref i,
                        "--output");
                    break;

                case "--output-root":
                    outputRoot = ReadValue(
                        args,
                        ref i,
                        "--output-root");
                    break;

                default:
                    throw new ArgumentException(
                        $"Argumento desconhecido: '{args[i]}'.");
            }
        }

        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException(
                "O argumento --id é obrigatório.");

        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException(
                "O argumento --source é obrigatório.");

        if (string.IsNullOrWhiteSpace(target))
            throw new ArgumentException(
                "O argumento --target é obrigatório.");

        if (string.IsNullOrWhiteSpace(output))
            throw new ArgumentException(
                "O argumento --output é obrigatório.");

        return new CreateOptions(
            id,
            version,
            source,
            target,
            output,
            outputRoot);
    }

    private static MappingOptions ParseMappingOptions(
        string[] args)
    {
        string? template = null;
        string? source = null;
        string? target = null;
        string? defaultValue = null;

        var required = false;
        var transform = TransformType.None;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--template":
                    template = ReadValue(
                        args,
                        ref i,
                        "--template");
                    break;

                case "--source":
                    source = ReadValue(
                        args,
                        ref i,
                        "--source");
                    break;

                case "--target":
                    target = ReadValue(
                        args,
                        ref i,
                        "--target");
                    break;

                case "--required":
                    required = true;
                    break;

                case "--default":
                    defaultValue = ReadValue(
                        args,
                        ref i,
                        "--default");
                    break;

                case "--transform":
                    var transformValue =
                        ReadValue(
                            args,
                            ref i,
                            "--transform");

                    if (!Enum.TryParse(
                        transformValue,
                        true,
                        out transform))
                    {
                        throw new ArgumentException(
                            $"Transform inválido: '{transformValue}'.");
                    }

                    break;

                default:
                    throw new ArgumentException(
                        $"Argumento desconhecido: '{args[i]}'.");
            }
        }

        if (string.IsNullOrWhiteSpace(template))
            throw new ArgumentException(
                "O argumento --template é obrigatório.");

        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException(
                "O argumento --source é obrigatório.");

        if (string.IsNullOrWhiteSpace(target))
            throw new ArgumentException(
                "O argumento --target é obrigatório.");

        return new MappingOptions(
            template,
            source,
            target,
            required,
            defaultValue,
            transform);
    }

    private static CollectionOptions ParseCollectionOptions(
        string[] args)
    {
        string? template = null;
        string? source = null;
        string? target = null;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--template":
                    template = ReadValue(
                        args,
                        ref i,
                        "--template");
                    break;

                case "--source":
                    source = ReadValue(
                        args,
                        ref i,
                        "--source");
                    break;

                case "--target":
                    target = ReadValue(
                        args,
                        ref i,
                        "--target");
                    break;

                default:
                    throw new ArgumentException(
                        $"Argumento desconhecido: '{args[i]}'.");
            }
        }

        if (string.IsNullOrWhiteSpace(template))
            throw new ArgumentException(
                "O argumento --template é obrigatório.");

        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException(
                "O argumento --source é obrigatório.");

        if (string.IsNullOrWhiteSpace(target))
            throw new ArgumentException(
                "O argumento --target é obrigatório.");

        return new CollectionOptions(
            template,
            source,
            target);
    }

    private static string ReadValue(
        string[] args,
        ref int index,
        string argument)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException(
                $"O argumento '{argument}' requer um valor.");
        }

        index++;

        var value = args[index];

        if (string.IsNullOrWhiteSpace(value) ||
            value.StartsWith("--"))
        {
            throw new ArgumentException(
                $"O argumento '{argument}' requer um valor.");
        }

        return value;
    }

    private sealed record CreateOptions(
        string Id,
        int Version,
        string Source,
        string Target,
        string Output,
        string? OutputRoot);

    private sealed record MappingOptions(
        string Template,
        string Source,
        string Target,
        bool Required,
        string? DefaultValue,
        TransformType Transform);

    private sealed record CollectionOptions(
        string Template,
        string Source,
        string Target);
}