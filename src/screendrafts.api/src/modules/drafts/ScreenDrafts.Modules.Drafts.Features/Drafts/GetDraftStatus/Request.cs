namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraftStatus;

internal sealed record Request
{
  [FromRoute(Name = "publicId")]
  public required string PublicId { get; init; }
}
