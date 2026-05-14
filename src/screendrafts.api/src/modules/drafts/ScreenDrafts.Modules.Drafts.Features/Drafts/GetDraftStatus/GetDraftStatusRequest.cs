namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraftStatus;

internal sealed record GetDraftStatusRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
}

