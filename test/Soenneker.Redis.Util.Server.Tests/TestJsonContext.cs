using System.Text.Json;
using System.Text.Json.Serialization;

namespace Soenneker.Redis.Util.Server.Tests;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(MetadataDocument))]
internal partial class TestJsonContext : JsonSerializerContext;

public sealed class MetadataDocument
{
    public string DisplayName { get; set; } = "";
}
