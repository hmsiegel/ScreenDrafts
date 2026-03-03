namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Get;

internal sealed record GetDrafterTeamResponse
{
  public required string PublicId { get; init; }
  public required string Name { get; init; }
  public int NumberOfDrafters { get; init; }
  public IReadOnlyList<GetDrafterTeamMemberResponse> Members { get; init; } = [];
}
