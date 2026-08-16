namespace Warp.Core.Templates;

/// <summary>
/// Um contrato de transformação — o "template" que dá nome ao WARP. Isso é
/// dado (YAML), não código: trocar uma regra de mapeamento é editar o
/// arquivo, nunca recompilar. Mesma filosofia da política ABAC do Corax.
/// <para/>
/// Versionamento é por convenção de nome de arquivo
/// (<c>csv-to-excel.v1.yaml</c>, <c>csv-to-excel.v2.yaml</c>) — ver
/// <see cref="TemplateLoader"/> e o README para a convenção completa.
/// Isso é deliberadamente simples: não existe resolução automática de
/// "versão mais recente" nem migração — cada consumidor aponta
/// explicitamente para o arquivo de versão que quer usar, e trocar de
/// versão é uma mudança de configuração visível, não implícita.
/// </summary>
public sealed class TemplateDefinition
{
    public string Id { get; set; } = "";
    public int Version { get; set; } = 1;
    public string SourceFormat { get; set; } = "";
    public string TargetFormat { get; set; } = "";
    public string? Description { get; set; }

    /// <summary>
    /// Nome do filho direto da raiz que representa UM registro repetido
    /// (ex: "row" para CSV, "item" para um array JSON). Se definido, o
    /// Transformer aplica os Mappings a CADA registro independentemente,
    /// produzindo uma coleção na saída — é o caso do CSV→Excel.
    /// <para/>
    /// Se null/vazio, o documento INTEIRO é tratado como um único registro
    /// (SourcePath dos mappings navega a partir da raiz) — é o caso de
    /// XML→cXML, onde a entrada é um pedido único, não uma tabela.
    /// <para/>
    /// Essa distinção é EXPLÍCITA no template, de propósito — nada de
    /// "engine adivinha se é tabela ou documento" por heurística. Mesmo
    /// princípio de explicit-over-implicit já usado no Corax (fail-closed,
    /// primeira regra que casa decide): comportamento ambíguo é pior que
    /// exigir uma linha a mais de configuração.
    /// </summary>
    public string? RecordsPath { get; set; }

    public List<FieldMapping> Mappings { get; set; } = new();
}