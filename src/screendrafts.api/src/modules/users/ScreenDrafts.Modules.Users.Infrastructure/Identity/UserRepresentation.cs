namespace ScreenDrafts.Modules.Users.Infrastructure.Identity;

internal sealed record UserRepresentation(
    string Username,
    string Email,
    string FirstName,
    string LastName,
    bool Enabled,
    bool EmailVerified,
    CredentialRepresentation[] Credentials,
    Dictionary<string, List<string>>? Attributes = null);
