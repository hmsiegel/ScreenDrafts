namespace ScreenDrafts.Modules.Reporting.Features.Drafts.ActivateSpotlight;

internal sealed class ActivateSpotlightRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
}
