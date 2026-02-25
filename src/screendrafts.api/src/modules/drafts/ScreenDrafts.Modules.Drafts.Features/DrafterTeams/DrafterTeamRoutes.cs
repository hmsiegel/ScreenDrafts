namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams;

internal static class DrafterTeamRoutes
{
  public const string Base = "/drafter-teams";
  public const string Membership = Base + "{publicId}/members";
  public const string MembershipWithDrafterId = Membership + "/{drafterId}";
}
