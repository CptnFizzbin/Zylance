using System.Text.RegularExpressions;

namespace Zylance.Core.Lib.Importers.Ofx.V1.Raw;

internal partial record OfxRawFile
{
    public List<OfxRawHeader> Headers { get; init; } = [];
    public required OfxRawElement Root { get; init; }

    public static OfxRawFile Parse(StreamReader content)
    {
        List<OfxRawHeader> headers = [];
        OfxRawElement? root = null;

        while (content.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line) || CommentLineRegex().IsMatch(line))
                continue;

            if (OfxRawHeader.IsMatch(line))
            {
                headers.Add(OfxRawHeader.ParseLine(line));
            }
            else if (OfxRawElement.IsStartLine(line))
            {
                if (root != null)
                    throw new InvalidDataException("Multiple root elements found in OFX file.");

                root = OfxRawElement.ParseElement(line, content);
            }
            else
            {
                throw new InvalidDataException($"Unexpected line in OFX file: {line}");
            }
        }

        if (root is null)
            throw new InvalidDataException("Missing root element.");

        return new OfxRawFile
        {
            Headers = headers,
            Root = root,
        };
    }

    [GeneratedRegex(@"^#")]
    private static partial Regex CommentLineRegex();
}
