namespace ScreenDrafts.Modules.Users.Domain.Abstractions.Identity;

public sealed record UserModel(
  string Email,
  string Password,
  string FirstName,
  string LastName,
  string? PublicId = null);
