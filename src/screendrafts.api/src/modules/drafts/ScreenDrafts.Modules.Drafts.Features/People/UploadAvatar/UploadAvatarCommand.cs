namespace ScreenDrafts.Modules.Drafts.Features.People.UploadAvatar;

internal sealed record UploadAvatarCommand : ICommand<UploadAvatarResponse>
{
  public required string PublicId { get; init; }
  public required Stream FileStream { get; init; }
  public required string FileName { get; init; }
  public required string ContentType { get; init; }
}
