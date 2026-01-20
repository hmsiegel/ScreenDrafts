namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraftStatus;

internal sealed record Response
{
  public string DraftPublicId { get; init; } = default!;
  public string DraftStatus { get; init; } = default!;
  public string Lifecycleview { get; init; } = default!;
  public IReadOnlyList<string> Actions { get; init; } = [];
  public int? ActionPartIndex { get; init; }
  public IReadOnlyList<DraftPartStatusResponse> Parts { get; init; } = [];
}
