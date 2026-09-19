namespace ScreenDrafts.Modules.Drafts.Features.Drafts.UploadImage;

internal sealed record UploadDraftImageRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; set; } = default!;
}
