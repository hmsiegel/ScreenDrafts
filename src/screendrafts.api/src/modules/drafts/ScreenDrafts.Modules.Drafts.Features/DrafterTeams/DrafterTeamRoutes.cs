namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams;

internal static class DrafterTeamRoutes
{
  public const string Base = "/drafter-teams";
  public const string Search = Base + "/search";
  public const string ById = Base + "/{publicId}";
  public const string Membership = ById + "/members";
  public const string MembershipWithDrafterId = Membership + "/{drafterId}";
}
