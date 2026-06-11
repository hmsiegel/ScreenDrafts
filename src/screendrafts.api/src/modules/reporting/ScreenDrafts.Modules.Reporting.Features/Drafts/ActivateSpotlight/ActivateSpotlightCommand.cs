namespace ScreenDrafts.Modules.Reporting.Features.Drafts.ActivateSpotlight;

internal sealed record ActivateSpotlightCommand : ICommand
{
  public required string PublicId { get; init; }
}
