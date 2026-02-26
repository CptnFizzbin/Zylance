using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zylance.Vault.Local.Entities;

/// <summary>
///     Entity Framework entity representing metadata in the _zylance_ marker
///     table.
///     This table identifies the database as a Zylance vault and stores metadata
///     fields.
/// </summary>
[Table("_zylance_")]
public class ZylanceMetadataEntity
{
    /// <summary>
    ///     Metadata key name.
    /// </summary>
    [Key]
    [MaxLength(255)]
    public required string Key { get; init; }

    /// <summary>
    ///     Metadata value.
    /// </summary>
    [MaxLength(255)]
    public required string Value { get; set; }
}
