namespace Warp.Core.Time;

/// <summary>
/// Existe por um motivo específico e importante: o WARP promete determinismo
/// ("mesmo input + mesmo template = mesmo output"), mas alguns formatos de
/// saída EXIGEM valor não-determinístico por definição de spec — um envelope
/// cXML válido precisa de <c>timestamp</c> e <c>payloadID</c> únicos a cada
/// transmissão (isso está na especificação cXML, não é escolha do WARP).
/// <para/>
/// Em vez de fingir que isso não existe (o que quebraria testes de
/// determinismo silenciosamente), a fronteira fica explícita: qualquer coisa
/// que precise do "agora" pede para uma <see cref="IClock"/> injetada, nunca
/// chama <c>DateTime.Now</c> direto. Em teste, injeta-se <see cref="FixedClock"/>
/// e o output volta a ser 100% reproduzível — o não-determinismo fica
/// isolado e visível, não espalhado pelo código.
/// </summary>
public interface IClock
{
    DateTimeOffset Now();
}

public sealed class SystemClock : IClock
{
    public DateTimeOffset Now() => DateTimeOffset.UtcNow;
}

/// <summary>Usado em testes — mesmo instante sempre, tornando até o envelope cXML reproduzível.</summary>
public sealed class FixedClock : IClock
{
    private readonly DateTimeOffset _fixedTime;
    public FixedClock(DateTimeOffset fixedTime) => _fixedTime = fixedTime;
    public DateTimeOffset Now() => _fixedTime;
}