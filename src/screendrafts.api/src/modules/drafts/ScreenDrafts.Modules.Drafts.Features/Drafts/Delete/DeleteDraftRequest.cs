namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Delete;

internal sealed record DeleteDraftRequest
{
  public string PublicId { get; init; } = default!;
}
