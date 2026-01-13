namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Delete;

internal sealed record Request
{
  public required string PublicId { get; init; }
}
