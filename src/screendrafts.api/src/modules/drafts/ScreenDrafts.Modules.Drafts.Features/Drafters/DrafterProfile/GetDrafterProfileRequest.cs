namespace ScreenDrafts.Modules.Drafts.Features.Drafters.DrafterProfile;

internal sealed record GetDrafterProfileRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
}
