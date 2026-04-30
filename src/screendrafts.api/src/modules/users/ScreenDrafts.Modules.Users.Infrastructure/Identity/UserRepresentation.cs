namespace ScreenDrafts.Modules.Users.Infrastructure.Identity;

internal sealed record UserRepresentation(
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("firstName")] string FirstName,
    [property: JsonPropertyName("lastName")] string LastName,
    [property: JsonPropertyName("enabled")] bool Enabled,
    [property: JsonPropertyName("emailVerified")] bool EmailVerified,
    [property: JsonPropertyName("credentials")] CredentialRepresentation[] Credentials,
    [property: JsonPropertyName("attributes")] Dictionary<string, List<string>>? Attributes = null);
