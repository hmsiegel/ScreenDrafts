namespace ScreenDrafts.Modules.Reporting.Features.Drafts.DeleteSpotlight;

internal sealed record DeleteSpotlightCommand : ICommand
{
  public required string PublicId { get; init; }
}
