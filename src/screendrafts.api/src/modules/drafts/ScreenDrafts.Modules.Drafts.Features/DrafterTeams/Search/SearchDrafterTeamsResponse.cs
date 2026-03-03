namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Search;

internal sealed record SearchDrafterTeamsResponse
{
  public required string PublicId { get; init; }
  public required string Name { get; init; }
  public int NumberOfDrafters { get; init; }
}
