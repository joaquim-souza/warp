namespace Warp.Core.Templates;

public static class TemplatePathValidator
{
    public static bool IsValid(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        // Não remover entradas vazias.
        // Isso permite detectar:
        // ".", ".product", "product." e "product..id".
        var segments = path.Split('.');

        if (segments.Length == 0)
        {
            return false;
        }

        foreach (var segment in segments)
        {
            if (string.IsNullOrWhiteSpace(segment))
            {
                return false;
            }

            if (!char.IsLetter(segment[0]) && segment[0] != '_')
            {
                return false;
            }

            if (!segment.All(
                c => char.IsLetterOrDigit(c) ||
                     c == '_' ||
                     c == '-'))
            {
                return false;
            }
        }

        return true;
    }
}