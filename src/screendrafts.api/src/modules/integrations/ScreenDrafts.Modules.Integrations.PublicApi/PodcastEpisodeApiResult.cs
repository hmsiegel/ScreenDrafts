namespace ScreenDrafts.Modules.Integrations.PublicApi;

public sealed record PodcastEpisodeApiResult
{
  public required string Title { get; init; }
  public required DateOnly ReleaseDate { get; init; }
  public required Uri Url { get; init; }
}
