namespace Warp.Core.Templates;

/// <summary>
/// Transformações puras aplicáveis a um valor durante o mapeamento.
/// Deliberadamente pequeno conjunto fechado (não é linguagem de script
/// arbitrária) — cada transform é auditável e testável isoladamente, e não
/// existe risco de um template rodar código arbitrário. Se um caso de uso
/// precisar de algo fora dessa lista, a resposta é adicionar um novo valor
/// aqui (revisável em PR), não abrir a porta pra expressão livre.
/// </summary>
public enum TransformType
{
    None,
    Trim,
    Upper,
    Lower,
    ToNumber,
    ToDateIso8601
}