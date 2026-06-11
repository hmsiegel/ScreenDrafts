namespace ScreenDrafts.Modules.Reporting.Features.Drafts.DeactivateSpotlight;

internal sealed record DeactivateSpotlightCommand : ICommand
{
  public required string PublicId { get; init; }
}
