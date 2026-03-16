namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.AddCandidateListEntry;

internal sealed record AddCanidateEntryResponse
{
  /// <summary>
  /// The internal ID of the candidate list entry. 
  /// </summary>
  public required Guid EntryId { get; init; }

  /// <summary>
  /// The TMDB ID of the candidate list entry.
  /// </summary>
  public required int TmdbId { get; init; }

  /// <summary>
  /// True when the movie record does not yet exist and a fetch has been queued.
  /// The entry will be reolved when MovieFetchedIntegrationEvent is consumed.
  /// </summary>
  public required bool IsPending { get; init; }
}
