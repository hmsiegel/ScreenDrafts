namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

internal sealed record GetDraftReleaseResponse
{
  public int ReleaseChannel { get; init; }
  public DateOnly ReleaseDate { get; init; }
}
