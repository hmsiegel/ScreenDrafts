namespace ScreenDrafts.Modules.Users.PublicApi;

public sealed record UserResponse(Guid UserId, string FirstName, string LastName, string? MiddleName = null);
