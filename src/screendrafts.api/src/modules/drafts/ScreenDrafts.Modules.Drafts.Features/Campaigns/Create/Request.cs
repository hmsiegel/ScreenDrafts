namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Create;

internal sealed record Request
{
  public required string Name { get; init; }
  public required string Slug { get; init; }
}


