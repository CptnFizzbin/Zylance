using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Zylance.Core.Settings.Services;

/// <summary>
///     Utility helpers for YAML serialization used by the Settings subsystem.
/// </summary>
public static class SettingsUtils
{
    /// <summary>
    ///     YAML serializer configured with camel-case naming convention.
    ///     Reuse this instance instead of creating new builders for each operation.
    /// </summary>
    public static readonly ISerializer YamlSerializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    /// <summary>
    ///     YAML deserializer configured with camel-case naming convention.
    ///     Reuse this instance instead of creating new builders for each operation.
    /// </summary>
    public static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();
}
