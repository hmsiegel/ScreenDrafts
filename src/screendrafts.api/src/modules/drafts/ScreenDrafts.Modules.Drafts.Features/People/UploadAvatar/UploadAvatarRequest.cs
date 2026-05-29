namespace ScreenDrafts.Modules.Drafts.Features.People.UploadAvatar;

internal sealed record UploadAvatarRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
}
