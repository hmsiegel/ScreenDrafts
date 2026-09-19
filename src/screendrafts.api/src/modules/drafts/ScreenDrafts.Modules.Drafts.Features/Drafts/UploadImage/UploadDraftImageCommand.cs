namespace ScreenDrafts.Modules.Drafts.Features.Drafts.UploadImage;

internal sealed record UploadDraftImageCommand : ICommand<UploadDraftImageResponse>
{
  public required string PublicId { get; set; }
  public required Stream FileStream { get; set; }
  public required string FileName { get; set; }
  public required string ContentType { get; set; }
}
