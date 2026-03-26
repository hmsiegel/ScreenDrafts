namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Get;

internal sealed record HostedDraftPartResponse
{
  public required string DraftPartPublicId { get; init; }
  public required string DraftPublicId { get; init; }
  public required string Label { get; init; }
  public required HostRole Role { get; init; }
  public required DraftPartStatus Status { get; init; }
}

