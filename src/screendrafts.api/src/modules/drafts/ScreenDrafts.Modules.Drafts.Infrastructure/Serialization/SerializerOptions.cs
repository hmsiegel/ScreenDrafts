using System.Text.Json;

namespace ScreenDrafts.Modules.Drafts.Infrastructure.Serialization;

internal static class SerializerOptions
{
  public static readonly JsonSerializerOptions Instance = new()
  {
    PropertyNameCaseInsensitive = true,
    Converters = { new DateOnlyJsonConverter() }
  };
}
