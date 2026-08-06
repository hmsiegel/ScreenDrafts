namespace ScreenDrafts.Modules.Integrations.Features.YouTube.SearchYouTube;

internal sealed record SearchYouTubeCommand : ICommand<SearchYouTubeResponse>
{
  public required string Query { get; init; }
  public int Page { get; init; } = 1;
}
