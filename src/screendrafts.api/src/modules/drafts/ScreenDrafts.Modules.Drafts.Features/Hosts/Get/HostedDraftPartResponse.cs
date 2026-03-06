namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Get;

internal sealed record HostedDraftPartResponse
{
  public required string DraftPartPublicId { get; init; }
  public required string DraftPublicId { get; init; }
  public required string Label { get; init; }
  public required string Role { get; init; }
  public required string Status { get; init; }
}

