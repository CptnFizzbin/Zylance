using System.Text.RegularExpressions;

namespace Zylance.Core.Lib.Importers.Ofx.Raw;

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
        startLine = startLine.Trim();
        var startMatch = ElementStartRegex().Match(startLine);
        var element = new OfxRawElement
        {
            Name = startMatch.Groups["Name"].Value.Trim().ToUpper(),
        };

        while (content.ReadLine() is { } nextLine)
        {
            nextLine = nextLine.Trim();
            
            // Skip empty lines
            if (string.IsNullOrWhiteSpace(nextLine))
                continue;
            
            if (ElementEndRegex().IsMatch(nextLine))
            {
                var endMatch = ElementEndRegex().Match(nextLine);
                var endName = endMatch.Groups["Name"].Value.Trim().ToUpper();
                return endName != element.Name
                    ? throw new InvalidDataException(
                        $"Mismatched end tag. Expected </{element.Name}>, found </{endName}>.")
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
        var child = Children.FirstOrDefault(c => c.Name == name);
        return child ?? throw new InvalidDataException($"Missing expected child element <{name}> in <{Name}>.");
    }
}
