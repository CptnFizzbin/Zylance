namespace Zylance.Core.Lib.Importers.Ofx.V1.Raw;

/// <summary>
/// A StreamReader that automatically trims whitespace from each line.
/// This ensures consistent handling of indented OFX files.
/// </summary>
internal class TrimmingStreamReader(Stream stream) : StreamReader(stream)
{
    public override string? ReadLine()
    {
        var line = base.ReadLine();
        return line?.Trim();
    }

    public override async Task<string?> ReadLineAsync()
    {
        var line = await base.ReadLineAsync();
        return line?.Trim();
    }

#if NET5_0_OR_GREATER
    public override async ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
    {
        var line = await base.ReadLineAsync(cancellationToken);
        return line?.Trim();
    }
#endif
}

