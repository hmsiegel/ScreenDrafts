namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Get;

internal sealed record GetDrafterTeamMemberResponse
{
  public required string PublicId { get; init; }
  public required string DisplayName { get; init; }
}
