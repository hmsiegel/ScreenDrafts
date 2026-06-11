namespace ScreenDrafts.Modules.Reporting.Features.Drafts.DeactivateSpotlight;

internal sealed class DeactivateSpotlightRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
}
