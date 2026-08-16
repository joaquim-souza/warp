namespace Warp.Core.Model;

/// <summary>
/// Envelope em torno da árvore canônica — carrega o formato de origem
/// junto, porque o Transformer às vezes precisa saber "isso veio de CSV"
/// para decidir como iterar (linha por linha) versus "isso veio de JSON/XML"
/// (nó por nó, estrutura arbitrária).
/// </summary>
public sealed class CanonicalDocument
{
    public CanonicalNode Root { get; }
    public string SourceFormat { get; }

    public CanonicalDocument(CanonicalNode root, string sourceFormat)
    {
        Root = root;
        SourceFormat = sourceFormat;
    }
}