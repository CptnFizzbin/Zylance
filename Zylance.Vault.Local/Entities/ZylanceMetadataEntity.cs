using System.ComponentModel.DataAnnotations;

namespace Zylance.Vault.Local.Entities;

/// <summary>
///     Entity Framework entity representing metadata in the _zylance_ marker table.
///     This table identifies the database as a Zylance vault and stores metadata fields.
/// </summary>
public class ZylanceMetadataEntity
{
    [Key]
    [MaxLength(255)]
    public required string Key { get; init; }

    [MaxLength(255)]
    public required string Value { get; set; }
}
