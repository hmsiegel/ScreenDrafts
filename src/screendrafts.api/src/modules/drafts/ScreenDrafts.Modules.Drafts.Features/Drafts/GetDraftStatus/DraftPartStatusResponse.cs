namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraftStatus;

internal sealed record DraftPartStatusResponse
{
  public Guid DraftPartId { get; init; }
  public int PartIndex { get; init; } = default!;
  public string Status { get; init; } = default!;
  public string Lifecycleview { get; init; } = default!;
  public DateTime? ScheduledForUtc { get; init; } = default!;
  public IReadOnlyList<string> Actions { get; init; } = [];
}
