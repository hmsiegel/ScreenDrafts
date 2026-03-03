namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Get;

internal sealed record GetDrafterTeamQuery : IQuery<GetDrafterTeamResponse>
{
  public required string PublicId { get; init; }
}
