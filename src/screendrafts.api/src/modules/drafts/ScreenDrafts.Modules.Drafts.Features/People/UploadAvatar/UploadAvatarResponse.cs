namespace ScreenDrafts.Modules.Drafts.Features.People.UploadAvatar;

internal sealed record UploadAvatarResponse
{
  public required string AvatarPath { get; init; }
}
