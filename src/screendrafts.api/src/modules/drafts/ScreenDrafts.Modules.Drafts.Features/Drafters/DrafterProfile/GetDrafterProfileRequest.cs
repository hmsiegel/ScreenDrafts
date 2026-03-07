namespace ScreenDrafts.Modules.Drafts.Features.Drafters.DrafterProfile;

internal sealed record GetDrafterProfileRequest
{
  [FromRoute(Name = "publicId")]
  public required string PublicId { get; init; }
}
