using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Zylance.Core.Lib.Extensions;

namespace Zylance.Core.Importers.Ofx.V1.Raw;

internal partial record OfxRawElement
{
    public required string Name { get; init; }
    public Dictionary<string, OfxRawToken> Tokens { get; } = [];
    public List<OfxRawElement> Children { get; } = [];

    public static bool IsStartLine(string line)
    {
        return ElementStartRegex().IsMatch(line);
    }

    public static OfxRawElement ParseElement(string startLine, StreamReader content)
    {
        var startMatch = ElementStartRegex().Match(startLine);
        var element = new OfxRawElement { Name = startMatch.Groups["Name"].Value.Trim().ToUpper() };

        while (content.ReadLineTrimmed() is { } nextLine)
        {
            if (string.IsNullOrWhiteSpace(nextLine))
                continue;

            if (ElementEndRegex().IsMatch(nextLine))
            {
                var endMatch = ElementEndRegex().Match(nextLine);
                var endName = endMatch.Groups["Name"].Value.Trim().ToUpper();
                return endName != element.Name
                    ? throw new InvalidDataException(
                        $"Mismatched end tag. Expected </{element.Name}>, found </{endName}>."
                    )
                    : element;
            }

            if (OfxRawToken.IsTokenLine(nextLine))
            {
                var token = OfxRawToken.ParseLine(nextLine);
                element.Tokens[token.Name] = token;
            }
            else if (IsStartLine(nextLine))
            {
                var childElement = ParseElement(nextLine, content);
                element.Children.Add(childElement);
            }
        }

        throw new InvalidDataException($"Unexpected end of data while parsing element <{element.Name}>.");
    }

    [GeneratedRegex(@"^\<(?'Name'[\w\d\.]+)\>$")]
    private static partial Regex ElementStartRegex();

    [GeneratedRegex(@"^\</(?'Name'[\w\d\.]+)\>$")]
    private static partial Regex ElementEndRegex();

    public OfxRawElement GetChildElement(string name)
    {
        return TryGetChildElement(name, out var childElement)
            ? childElement
            : throw new InvalidDataException($"Missing expected child element <{name}> in <{Name}>.");
    }

    public bool TryGetChildElement(string name, [NotNullWhen(true)] out OfxRawElement? childElement)
    {
        childElement = Children.FirstOrDefault(c => c.Name == name);
        return childElement is not null;
    }

    public OfxRawToken GetToken(string name)
    {
        return TryGetToken(name, out var token)
            ? token
            : throw new InvalidDataException($"Missing expected token {name} in element <{Name}>.");
    }

    public bool TryGetToken(string name, [NotNullWhen(true)] out OfxRawToken? token)
    {
        return Tokens.TryGetValue(name, out token);
    }
}
