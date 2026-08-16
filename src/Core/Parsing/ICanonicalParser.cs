using Warp.Core.Model;

namespace Warp.Core.Parsing;

/// <summary>
/// Contrato de parsing: qualquer formato de entrada implementa isso e vira
/// um <see cref="CanonicalDocument"/>. Novo formato de entrada = nova
/// implementação dessa interface, nada mais no resto do engine precisa mudar
/// — é o mesmo princípio de "trocar implementação sem tocar em quem consome"
/// que já apareceu no Corax (PolicyEngine) e no Falcon (ICorrelationEngine).
/// </summary>
public interface ICanonicalParser
{
    /// <summary>Nome do formato que esse parser entende (ex: "csv", "json", "xml") — usado pelo registry do engine.</summary>
    string FormatName { get; }

    CanonicalDocument Parse(Stream input);
}