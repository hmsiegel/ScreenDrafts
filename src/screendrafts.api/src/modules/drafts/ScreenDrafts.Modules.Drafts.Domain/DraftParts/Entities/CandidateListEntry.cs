using System.Data;

namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;

public sealed class CandidateListEntry : Entity<CandidateListEntryId>
{
  private CandidateListEntry(
    DraftPartId draftPartId,
    string draftPartPublicId,
    int tmdbId,
    Guid? movieId,
    string addedByPublicId,
    string? notes,
    CandidateListEntryId? id = null)
    : base(id ?? CandidateListEntryId.CreateUnique())
  {
    DraftPartId = draftPartId;
    DraftPartPublicId = draftPartPublicId;
    TmdbId = tmdbId;
    MovieId = movieId;
    AddedByPublicId = addedByPublicId;
    Notes = notes;
    CreatedOnUtc = DateTime.UtcNow;
    IsPending = movieId is null;
  }

  private CandidateListEntry() { }

  public DraftPartId DraftPartId { get; private set; } = default!;
  public string DraftPartPublicId { get; private set; } = default!;

  /// <summary>
  /// The TMDB ID used to identify this entry. Retained ase the stable extetnal key
  /// even after the movie record is created.
  /// </summary>
  public int TmdbId { get; private set; }

  /// <summary>
  /// Null until the Movies module processes the FetchMovieRequestedIntegrationEvent
  /// and the MovieFetchedIntegrationEvent is consumed.
  /// </summary>
  public Guid? MovieId { get; private set; }
  public string AddedByPublicId { get; private set; } = default!;
  public string? Notes { get; private set; }
  public DateTime CreatedOnUtc { get; private set; }

  /// <summary>
  /// True while the movie record has not yet been created. 
  /// Cleared by the Movies module when processing the MovieFetchedIntegrationEvent.
  /// </summary>
  public bool IsPending { get; private set; }

  public static Result<CandidateListEntry> Create(
    DraftPartId draftPartId,
    string draftPartPublicId,
    int tmdbId,
    Guid? movieId,
    string addedByPublicId,
    string? notes = null,
    CandidateListEntryId? id = null)
  {
    var entry = new CandidateListEntry(
      draftPartId: draftPartId,
      draftPartPublicId: draftPartPublicId,
      tmdbId: tmdbId,
      movieId: movieId,
      addedByPublicId: addedByPublicId,
      notes: notes,
      id: id);

    return Result.Success(entry);
  }

  public Result Resolve(Guid movieId)
  {
    if (!IsPending)
    {
      return Result.Failure(CandidateListErrors.EntryAlreadyResolved);
    }

    MovieId = movieId;
    IsPending = false;

    return Result.Success();
  }
}
