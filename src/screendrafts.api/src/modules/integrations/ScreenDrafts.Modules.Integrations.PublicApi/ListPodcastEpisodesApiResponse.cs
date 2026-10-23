namespace ScreenDrafts.Modules.Integrations.PublicApi;

public sealed record ListPodcastEpisodesApiResponse
{
  public IReadOnlyList<PodcastEpisodeApiResult> Results { get; init; } = [];
}
