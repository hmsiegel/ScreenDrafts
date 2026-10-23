namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

internal sealed record GetDraftReleaseLinkResponse
{
  public string Platform { get; init; } = default!;
  public Uri Url { get; init; } = default!;
}
