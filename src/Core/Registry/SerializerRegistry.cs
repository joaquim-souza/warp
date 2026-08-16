using Warp.Core.Serialization;

namespace Warp.Core.Registry;

/// <summary>
/// Registro dos serializers disponíveis no WARP.
/// </summary>
public sealed class SerializerRegistry
{
    private readonly Dictionary<string, ICanonicalSerializer> _serializers =
        new(StringComparer.OrdinalIgnoreCase);

    public SerializerRegistry(IEnumerable<ICanonicalSerializer> serializers)
    {
        foreach (var serializer in serializers)
        {
            Register(serializer);
        }
    }

    public void Register(ICanonicalSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);

        if (string.IsNullOrWhiteSpace(serializer.FormatName))
        {
            throw new ArgumentException(
                "Serializer deve possuir um FormatName.",
                nameof(serializer));
        }

        if (!_serializers.TryAdd(serializer.FormatName, serializer))
        {
            throw new InvalidOperationException(
                $"Já existe um serializer registrado para o formato '{serializer.FormatName}'.");
        }
    }

    public ICanonicalSerializer Get(string format)
    {
        if (string.IsNullOrWhiteSpace(format))
        {
            throw new ArgumentException(
                "Formato não pode ser vazio.",
                nameof(format));
        }

        if (!_serializers.TryGetValue(format, out var serializer))
        {
            throw new KeyNotFoundException(
                $"Nenhum serializer registrado para o formato '{format}'.");
        }

        return serializer;
    }

    public bool Contains(string format) =>
        !string.IsNullOrWhiteSpace(format) &&
        _serializers.ContainsKey(format);

    public IReadOnlyCollection<string> Formats =>
        _serializers.Keys.ToList().AsReadOnly();
}