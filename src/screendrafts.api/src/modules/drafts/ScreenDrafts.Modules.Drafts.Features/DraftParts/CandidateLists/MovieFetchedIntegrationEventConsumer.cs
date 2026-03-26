namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists;

internal sealed class MovieFetchedIntegrationEventConsumer(ICandidateListRepository candidateListRepository)
  : IConsumer<MediaFetchedIntegrationEvent>
{
  private readonly ICandidateListRepository _candidateListRepository = candidateListRepository;

  public async Task Consume(ConsumeContext<MediaFetchedIntegrationEvent> context)
  {
    var tmdbId = context.Message.TmdbId!.Value;

    var movieId = await _candidateListRepository.FindMovieByTmdbIdAsync(
      tmdbId,
      context.CancellationToken);

    if (movieId is null)
    {
      return;
    }

    var pendingEntries = await _candidateListRepository.GetPendingEntriesByTmdbIdAsync(
      tmdbId,
      context.CancellationToken);

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
