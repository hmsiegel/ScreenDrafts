namespace ScreenDrafts.Common.Infrastructure.Identity;

public sealed record CredentialRepresentation(
  [property: JsonPropertyName("type")] string Type,
  [property: JsonPropertyName("value")] string Value,
  [property: JsonPropertyName("temporary")] bool Temporary
);
