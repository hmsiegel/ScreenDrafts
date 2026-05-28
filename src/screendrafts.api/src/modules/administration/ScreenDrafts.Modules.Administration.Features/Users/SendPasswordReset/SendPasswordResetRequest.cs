namespace ScreenDrafts.Modules.Administration.Features.Users.SendPasswordReset;

internal sealed record SendPasswordResetRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
}
