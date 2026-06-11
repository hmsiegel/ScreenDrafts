namespace ScreenDrafts.Modules.Reporting.Features.Drafts.DeleteSpotlight;

internal sealed class DeleteSpotlightRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
}
