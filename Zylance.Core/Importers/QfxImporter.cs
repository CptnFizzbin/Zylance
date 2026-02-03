using Zylance.Core.Lib.Importers;

namespace Zylance.Core.Importers;

/// <summary>
/// Importer for QFX (Quicken Financial Exchange) format files.
/// QFX is an OFX-based format used by Quicken for financial data exchange.
/// </summary>
public class QfxImporter : IImporter
{
    /// <inheritdoc />
    public IReadOnlyList<string> SupportedExtensions { get; } = new[] { ".qfx" };

    /// <inheritdoc />
    public Task<bool> CanImportAsync(string filePath)
    {
        // Stub implementation - actual logic to be implemented
        throw new NotImplementedException("QfxImporter.CanImportAsync is not yet implemented.");
    }

    /// <inheritdoc />
    public Task<ImportResult> ImportAsync(string filePath, CancellationToken cancellationToken = default)
    {
        // Stub implementation - actual logic to be implemented
        throw new NotImplementedException("QfxImporter.ImportAsync is not yet implemented.");
    }
}
