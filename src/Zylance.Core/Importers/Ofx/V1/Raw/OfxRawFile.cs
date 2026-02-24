using System.Text;
using System.Text.RegularExpressions;
using Zylance.Core.Importers.Ofx.Extensions;

namespace Zylance.Core.Importers.Ofx.V1.Raw;

internal partial record OfxRawFile
{
    public List<OfxRawHeader> Headers { get; init; } = [];
    public required OfxRawElement Root { get; init; }

    private static StreamReader NormalizeStream(StreamReader content)
    {
        // Read all lines from the input stream
        var lines = new List<string>();
        string? line;
        while ((line = content.ReadLine()) is not null)
        {
            if (!line.Contains('<'))
            {
                lines.Add(line);
                continue;
            }

            // Split each line on '<' to ensure every tag starts a new line
            var chunks = line.Replace("<", "\n<")
                .Split('\n')
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrEmpty(l));
            lines.AddRange(chunks);
        }

        var normalized = string.Join(Environment.NewLine, lines);
        var memStream = new MemoryStream(Encoding.UTF8.GetBytes(normalized));
        return new StreamReader(memStream);
    }

    public static OfxRawFile Parse(StreamReader content)
    {
        var normalizedContent = NormalizeStream(content);
        List<OfxRawHeader> headers = [];
        OfxRawElement? root = null;

        while (normalizedContent.ReadLineTrimmed() is { } line)
        {
            Console.WriteLine("Processing line: {0} ", line);
            if (string.IsNullOrWhiteSpace(line) || CommentLineRegex().IsMatch(line))
                continue;

            if (OfxRawHeader.IsMatch(line))
            {
                headers.Add(OfxRawHeader.ParseLine(line));
            }
            else if (OfxRawElement.IsStartLine(line))
            {
                if (root is not null)
                    throw new InvalidDataException("Multiple root elements found in OFX file.");

                root = OfxRawElement.ParseElement(line, normalizedContent);
            }
            else
            {
                throw new InvalidDataException($"Unexpected line in OFX file: {line}");
            }
        }

        if (root is null)
            throw new InvalidDataException("Missing root element.");

        return new OfxRawFile { Headers = headers, Root = root };
    }

    [GeneratedRegex(@"^#")]
    private static partial Regex CommentLineRegex();
}
