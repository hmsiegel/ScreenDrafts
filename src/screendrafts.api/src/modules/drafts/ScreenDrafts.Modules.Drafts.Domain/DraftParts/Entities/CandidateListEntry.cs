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
    CandidateListEntryId? id = null
  )
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

  /// <summary>
  /// True once a pick has claimed this entry. Candidate lists are
  /// inexhaustive and shared — picking a listed movie does not remove it
  /// from view, but it does mark it picked so it can be visually
  /// distinguished and restored cleanly if the pick is later vetoed.
  /// </summary>
  public bool IsPicked { get; private set; }

  /// <summary>
  /// The pick that claimed this entry. Null until MarkAsPicked is called;
  /// cleared again by RestoreToAvailable if that pick is vetoed.
  /// </summary>
  public PickId? PickId { get; private set; }

  public static Result<CandidateListEntry> Create(
    DraftPartId draftPartId,
    string draftPartPublicId,
    int tmdbId,
    Guid? movieId,
    string addedByPublicId,
    string? notes = null,
    CandidateListEntryId? id = null
  )
  {
    var entry = new CandidateListEntry(
      draftPartId: draftPartId,
      draftPartPublicId: draftPartPublicId,
      tmdbId: tmdbId,
      movieId: movieId,
      addedByPublicId: addedByPublicId,
      notes: notes,
      id: id
    );

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

  /// <summary>
  /// Claims this entry for a pick. Does not remove the entry from the list —
  /// candidate lists are inexhaustive and shared, so the entry remains
  /// visible, just flagged as picked.
  /// </summary>
  public Result MarkAsPicked(PickId pickId)
  {
    if (IsPicked)
    {
      return Result.Failure(CandidateListErrors.EntryAlreadyPicked);
    }

    IsPicked = true;
    PickId = pickId;

    return Result.Success();
  }

  /// <summary>
  /// Reverses MarkAsPicked. Called when the pick that claimed this entry is
  /// vetoed, returning the entry to available status so it can be picked
  /// again by anyone.
  /// </summary>
  public Result RestoreToAvailable()
  {
    if (!IsPicked)
    {
      return Result.Failure(CandidateListErrors.EntryNotPicked);
    }

    IsPicked = false;
    PickId = null;

    return Result.Success();
  }
}
