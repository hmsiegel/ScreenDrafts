namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;

/// <summary>
/// Reserves a specific film for a specific drafter to play on the Legends community's
/// behalf — the "Booster's Champion" pick in a Legends Mega draft. Unlike
/// CommunityFilmRule's BoostersPick rule kind (which enforces exact board-slot placement
/// via a reactive auto-veto at reveal time), this enforces WHO is allowed to play the film
/// at all, checked at PlayPick time before the pick is even created — the drafter can play
/// it at any slot they choose. Deliberately a separate, simpler mechanism rather than a
/// third CommunityFilmRuleKind value, since the two don't share behavior: no slot
/// constraint, a hard block instead of an after-the-fact veto, and attribution to one
/// named drafter rather than the generic Community participant. Uses Entity (plain Guid
/// Id) rather than a strongly-typed Entity&lt;TId&gt;, mirroring CommunityFilmRule's own
/// choice for this kind of simple DraftPart-owned child record.
/// </summary>
public sealed class BoostersChampionAssignment : Entity
{
  private BoostersChampionAssignment(
    string publicId,
    Guid assignedDrafterIdValue,
    int? tmdbId,
    Guid? id = null
  )
    : base(id ?? Guid.NewGuid())
  {
    PublicId = publicId;
    AssignedDrafterIdValue = assignedDrafterIdValue;
    TmdbId = tmdbId;
  }

  private BoostersChampionAssignment() { }

  public string PublicId { get; private set; } = default!;

  /// <summary>
  /// The drafter reserved to play this pick — a bare Guid, not a Participant, since a
  /// Booster's Champion is always a Drafter by definition, never a Team or Community.
  /// </summary>
  public Guid AssignedDrafterIdValue { get; private set; }

  /// <summary>
  /// The reserved film, once known. Nullable — the champion can be assigned before the
  /// title is decided, mirroring CommunityFilmRule.TmdbId's own nullability.
  /// </summary>
  public int? TmdbId { get; private set; }

  public static Result<BoostersChampionAssignment> Create(
    string publicId,
    Guid assignedDrafterIdValue,
    int? tmdbId = null,
    Guid? id = null
  )
  {
    if (string.IsNullOrWhiteSpace(publicId))
    {
      return Result.Failure<BoostersChampionAssignment>(
        BoostersChampionAssignmentErrors.CreationFailed
      );
    }

    if (assignedDrafterIdValue == Guid.Empty)
    {
      return Result.Failure<BoostersChampionAssignment>(
        BoostersChampionAssignmentErrors.AssignedDrafterRequired
      );
    }

    return Result.Success(
      new BoostersChampionAssignment(publicId, assignedDrafterIdValue, tmdbId, id)
    );
  }

  internal void AssignFilm(int tmdbId)
  {
    TmdbId = tmdbId;
  }

  internal void ClearFilm()
  {
    TmdbId = null;
  }
}
