namespace ScreenDrafts.Modules.Users.Infrastructure.Identity;

internal sealed record CredentialRepresentation(
  [property: JsonPropertyName("type")] string Type,
  [property: JsonPropertyName("value")] string Value,
  [property: JsonPropertyName("temporary")] bool Temporary);
