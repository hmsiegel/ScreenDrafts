namespace ScreenDrafts.Common.Infrastructure.Identity;

public sealed record UserEmailUpdateRepresentation(
  [property: JsonPropertyName("email")] string Email,
  [property: JsonPropertyName("emailVerified")] bool EmailVerified
);
