using System.Net;
using ScreenDrafts.Modules.Integrations.Domain.Services.Podcasts;

namespace ScreenDrafts.Modules.Integrations.Features.Podcasts.ListPodcastEpisodes;

internal sealed record ListPodcastEpisodesCommand : ICommand<ListPodcastEpisodesResponse>
{
  public required PodcastPlatform Platform { get; init; }
  public required DateOnly ReleasedOnOrAfter { get; init; }
}

internal sealed record ListPodcastEpisodesResponse
{
  public IReadOnlyList<PodcastEpisode> Episodes { get; init; } = [];
}

internal sealed class ListPodcastEpisodesCommandHandler(
  ISpotifyService spotifyService,
  IApplePodcastsService applePodcastsService
) : ICommandHandler<ListPodcastEpisodesCommand, ListPodcastEpisodesResponse>
{
  private readonly ISpotifyService _spotifyService = spotifyService;
  private readonly IApplePodcastsService _applePodcastsService = applePodcastsService;

  public async Task<Result<ListPodcastEpisodesResponse>> Handle(
    ListPodcastEpisodesCommand request,
    CancellationToken cancellationToken
  )
  {
    var platform = request.Platform.ToString();

    try
    {
      var episodes = await ListAsync(request, cancellationToken);

      return Result.Success(new ListPodcastEpisodesResponse { Episodes = episodes });
    }
    catch (HttpRequestException exception)
      when (exception.StatusCode == HttpStatusCode.TooManyRequests)
    {
      return Result.Failure<ListPodcastEpisodesResponse>(PodcastErrors.RateLimited(platform));
    }
    catch (HttpRequestException exception)
    {
      return Result.Failure<ListPodcastEpisodesResponse>(
        PodcastErrors.RequestFailed(platform, exception.Message)
      );
    }
    catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
    {
      // HttpClient timeout, not a caller cancellation.
      return Result.Failure<ListPodcastEpisodesResponse>(
        PodcastErrors.RequestFailed(platform, "The request timed out.")
      );
    }
  }

  private Task<IReadOnlyList<PodcastEpisode>> ListAsync(
    ListPodcastEpisodesCommand request,
    CancellationToken cancellationToken
  ) =>
    request.Platform switch
    {
      PodcastPlatform.Spotify => _spotifyService.ListEpisodesAsync(
        request.ReleasedOnOrAfter,
        cancellationToken
      ),
      PodcastPlatform.Apple => _applePodcastsService.ListEpisodesAsync(
        request.ReleasedOnOrAfter,
        cancellationToken
      ),
      _ => throw new ArgumentOutOfRangeException(
        nameof(request),
        request.Platform,
        "Unsupported podcast platform."
      ),
    };
}
