namespace Warp.Core.Model;

/// <summary>
/// Um nó da árvore canônica — a representação intermediária que CSV, JSON e
/// XML viram depois de parseados, e a partir da qual qualquer formato de
/// saída é gerado. Isso é o coração do WARP: em vez de N×N conversores
/// diretos entre cada par de formato, todo mundo converte PARA isso e
/// qualquer formato converte A PARTIR disso.
/// <para/>
/// Modelo propositalmente simples — nome, valor opcional, atributos (para
/// coisas tipo atributo XML), e filhos ordenados. Suficiente para
/// representar tanto uma linha de CSV (nó "row" com um filho por coluna)
/// quanto uma árvore JSON/XML arbitrariamente aninhada.
/// <para/>
/// Ordem dos filhos é sempre preservada (List, nunca Dictionary/HashSet) —
/// isso é requisito para determinismo: a mesma entrada precisa produzir a
/// mesma árvore, na mesma ordem, sempre.
/// </summary>
public sealed class CanonicalNode
{
    public string Name { get; }
    public string? Value { get; set; }
    public Dictionary<string, string> Attributes { get; } = new();
    public List<CanonicalNode> Children { get; } = new();

    public CanonicalNode(string name, string? value = null)
    {
        Name = name;
        Value = value;
    }

    public CanonicalNode AddChild(CanonicalNode child)
    {
        Children.Add(child);
        return this;
    }

    public CanonicalNode AddChild(string name, string? value = null)
    {
        var child = new CanonicalNode(name, value);
        Children.Add(child);
        return child;
    }

    /// <summary>Primeiro filho direto com esse nome, ou null. Não busca recursivamente.</summary>
    public CanonicalNode? Child(string name) => Children.FirstOrDefault(c => c.Name == name);

    /// <summary>Todos os filhos diretos com esse nome — usado para listas (ex: várias "row").</summary>
    public IEnumerable<CanonicalNode> ChildrenNamed(string name) => Children.Where(c => c.Name == name);

    /// <summary>
    /// Navega por um caminho tipo "row.price" (separado por ponto),
    /// pegando o PRIMEIRO filho em cada nível. Usado pelo Transformer para
    /// resolver o SourcePath de um FieldMapping.
    /// </summary>
    public CanonicalNode? Navigate(string dottedPath)
    {
        var current = this;
        foreach (var segment in dottedPath.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            current = current?.Child(segment);
            if (current is null) return null;
        }
        return current;
    }
}