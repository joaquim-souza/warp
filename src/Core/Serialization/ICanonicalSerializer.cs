using Warp.Core.Model;

namespace Warp.Core.Serialization;

/// <summary>
/// Contrato de serialização — espelha <see cref="Warp.Core.Parsing.ICanonicalParser"/>
/// do outro lado do pipeline. Novo formato de saída = nova implementação,
/// nada mais precisa mudar.
/// </summary>
public interface ICanonicalSerializer
{
    string FormatName { get; }
    void Serialize(CanonicalDocument document, Stream output);
}