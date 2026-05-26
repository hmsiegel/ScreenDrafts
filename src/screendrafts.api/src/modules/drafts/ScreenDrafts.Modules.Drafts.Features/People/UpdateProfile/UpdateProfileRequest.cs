namespace ScreenDrafts.Modules.Drafts.Features.People.UpdateProfile;

internal sealed record UpdateProfileRequest
{
  [FromRoute(Name = "publicId")]
  public required string PublicId { get; init; }

  public string? DisplayName { get; init; }
  public string? Biography { get; init; }
  public string? Location { get; init; }
}
