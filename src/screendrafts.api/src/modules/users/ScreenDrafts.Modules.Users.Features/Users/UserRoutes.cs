namespace ScreenDrafts.Modules.Users.Features.Users;

internal static class UserRoutes
{
  public const string Base = "/users";
  public const string Register = Base + "/register";
  public const string Social = Register + "/social";
  public const string GetById = Base + "/{userId:guid}";
  public const string Profile = Base + "/profile";
  public const string PublicProfiles = Base + "/public-profiles/by-person-ids";
  public const string Password = Profile + "/password";
  public const string EmailChangeBase = Base + "/email-change";

  // Bootstrap (fake -> real, one-time migration)
  public const string EmailChangeBootstrapCandidates = EmailChangeBase + "/bootstrap/candidates";
  public const string EmailChangeBootstrapGenerate = EmailChangeBase + "/bootstrap/generate";
  public const string EmailChangeBootstrapValidate = EmailChangeBase + "/bootstrap/validate";
  public const string EmailChangeBootstrapClaim = EmailChangeBase + "/bootstrap/claim";

  // Steady-state (real -> real, ongoing)
  public const string EmailChangeRequest = EmailChangeBase + "/request";
  public const string EmailChangeConfirm = EmailChangeBase + "/confirm";
}
