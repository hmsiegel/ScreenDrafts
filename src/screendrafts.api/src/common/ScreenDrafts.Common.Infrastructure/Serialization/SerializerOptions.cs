namespace ScreenDrafts.Common.Infrastructure.Serialization;

public static class SerializerOptions
{
  public static readonly JsonSerializerOptions Instance = new()
  {
    PropertyNameCaseInsensitive = true,
    Converters = { new DateOnlyJsonConverter() },
    NumberHandling = JsonNumberHandling.AllowReadingFromString
  };
}
