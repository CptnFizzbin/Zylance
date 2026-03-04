using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Zylance.Core.Settings.Services;

public static class SettingsUtils
{
    public static readonly ISerializer YamlSerializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    public static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();
}
