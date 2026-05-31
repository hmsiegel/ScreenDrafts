namespace ScreenDrafts.Modules.Drafts.Features.Drafters.Search;

internal sealed record SearchDraftersResponse
{
  public required string PublicId { get; init; }
  public required string PersonPublicId { get; init; }
  public required string DisplayName { get; init; }
  public required bool IsRetired { get; init; }
}
