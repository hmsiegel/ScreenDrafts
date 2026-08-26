namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts;

public sealed partial class DraftPart
{
  private readonly List<BoostersChampionAssignment> _boostersChampionAssignments = [];

  public IReadOnlyList<BoostersChampionAssignment> BoostersChampionAssignments =>
    _boostersChampionAssignments.AsReadOnly();

  /// <summary>
  /// Reserves a specific drafter as this Legends Mega part's Booster's Champion. The film
  /// itself is optional here — see AssignFilmToBoostersChampionAssignment — since the
  /// champion is often assigned before the title is decided.
  /// </summary>
  public Result AddBoostersChampionAssignment(
    string publicId,
    Guid assignedDrafterIdValue,
    int? tmdbId = null
  )
  {
    if (Status != DraftPartStatus.Created)
    {
      return Result.Failure(BoostersChampionAssignmentErrors.NotModifiable);
    }

    var assignedParticipant = new Participant(assignedDrafterIdValue, ParticipantKind.Drafter);

    if (!HasParticipant(assignedParticipant))
    {
      return Result.Failure(
        DraftPartErrors.ParticipantDoesNotBelongToThisDraftPart(assignedParticipant)
      );
    }

    var result = BoostersChampionAssignment.Create(publicId, assignedDrafterIdValue, tmdbId);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _boostersChampionAssignments.Add(result.Value);
    UpdatedAtUtc = DateTime.UtcNow;

    return Result.Success();
  }

  /// <summary>
  /// Sets or changes the reserved film for an existing assignment. Gated the same as Add/
  /// Remove (Status == Created only) — matches CommunityFilmRule.AssignFilmToCommunityFilmRule's
  /// own precedent, so the title has to be entered before the part starts even if other
  /// drafters don't find out until it's actually played.
  /// </summary>
  public Result AssignFilmToBoostersChampionAssignment(string publicId, int tmdbId)
  {
    if (Status != DraftPartStatus.Created)
    {
      return Result.Failure(BoostersChampionAssignmentErrors.NotModifiable);
    }

    var assignment = _boostersChampionAssignments.FirstOrDefault(a => a.PublicId == publicId);

    if (assignment is null)
    {
      return Result.Failure(BoostersChampionAssignmentErrors.NotFound(publicId));
    }

    if (_boostersChampionAssignments.Any(a => a.TmdbId == tmdbId && a.PublicId != publicId))
    {
      return Result.Failure(BoostersChampionAssignmentErrors.FilmAlreadyAssigned(tmdbId));
    }

    assignment.AssignFilm(tmdbId);
    UpdatedAtUtc = DateTime.UtcNow;

    return Result.Success();
  }

  public Result RemoveBoostersChampionAssignment(string publicId)
  {
    if (Status != DraftPartStatus.Created)
    {
      return Result.Failure(BoostersChampionAssignmentErrors.NotModifiable);
    }

    var assignment = _boostersChampionAssignments.FirstOrDefault(a => a.PublicId == publicId);

    if (assignment is null)
    {
      return Result.Failure(BoostersChampionAssignmentErrors.NotFound(publicId));
    }

    _boostersChampionAssignments.Remove(assignment);
    UpdatedAtUtc = DateTime.UtcNow;

    return Result.Success();
  }
}
