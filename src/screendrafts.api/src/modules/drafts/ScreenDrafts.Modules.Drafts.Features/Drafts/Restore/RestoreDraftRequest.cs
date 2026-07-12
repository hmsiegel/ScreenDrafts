namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Restore;

internal sealed record RestoreDraftRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
}
