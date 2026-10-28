namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists;

internal sealed class MovieFetchedIntegrationEventConsumer(
  ICandidateListRepository candidateListRepository
) : IntegrationEventHandler<MediaFetchedIntegrationEvent>
{
  private readonly ICandidateListRepository _candidateListRepository = candidateListRepository;

  public override async Task Handle(
    MediaFetchedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    var tmdbId = integrationEvent.TmdbId!.Value;

    var movieId = await _candidateListRepository.FindMovieByTmdbIdAsync(
      tmdbId,
      CancellationToken.None
    );

    if (movieId is null)
    {
      return;
    }

    var pendingEntries = await _candidateListRepository.GetPendingEntriesByTmdbIdAsync(
      tmdbId,
      CancellationToken.None
    );

    foreach (var entry in pendingEntries)
    {
      entry.Resolve(movieId.Value);
    }

    if (pendingEntries.Count > 0)
    {
      _candidateListRepository.UpdateRange(pendingEntries);
    }
  }
}
