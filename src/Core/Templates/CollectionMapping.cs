namespace Warp.Core.Templates;

/// <summary>
/// Define a transformação de uma coleção de elementos.
///
/// Cada elemento encontrado em SourcePath gera um novo nó no TargetPath.
/// Todos os mappings internos são aplicados ao mesmo elemento de destino,
/// preservando a associação entre os campos daquele item.
/// </summary>
public sealed class CollectionMapping
{
    public string SourcePath { get; set; } = "";

    public string TargetPath { get; set; } = "";

    public bool Required { get; set; } = true;

    public List<FieldMapping> Mappings { get; set; } = [];
}