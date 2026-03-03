namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

internal sealed record GetDraftHostResponse
{
  public string HostPublicId { get; init; } = default!;
  public string DisplayName { get; init; } = default!;
}
