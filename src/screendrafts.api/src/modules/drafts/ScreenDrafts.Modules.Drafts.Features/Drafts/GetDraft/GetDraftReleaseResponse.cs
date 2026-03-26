namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

internal sealed record GetDraftReleaseResponse
{
  public ReleaseChannel ReleaseChannel { get; init; } = default!;
  public DateOnly ReleaseDate { get; init; }
}
