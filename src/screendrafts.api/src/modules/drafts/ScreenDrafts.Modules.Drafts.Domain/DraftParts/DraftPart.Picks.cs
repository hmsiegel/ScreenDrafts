namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts;

public sealed partial class DraftPart
{
  public Result<PickId> PlayPick(
    ISeriesPolicyProvider seriesPolicyProvider,
    SeriesId seriesId,
    DraftType draftType,
    Movie movie,
    int draftPosition,
    int playOrder,
    ParticipantId participantId,
    string? movieVersionName = null,
    Func<Guid, bool>? isMovieAlreadyPickedInWholeDraft = null)
  {
    ArgumentNullException.ThrowIfNull(seriesPolicyProvider);
    ArgumentNullException.ThrowIfNull(seriesId);
    ArgumentNullException.ThrowIfNull(movie);

    if (Status != DraftPartStatus.InProgress)
    {
      return Result.Failure<PickId>(DraftPartErrors.DraftNotStarted);
    }

    if (participantId.Kind == ParticipantKind.Community)
    {
      var budget = ResolvePartBudget(seriesPolicyProvider, seriesId, draftType);

      if (budget.MaxCommunityPicks <= 0)
      {
        return Result.Failure<PickId>(DraftPartErrors.CommunityPicksNotAllowed);
      }

      if (CommunityPicksUsed >= budget.MaxCommunityPicks)
      {
        return Result.Failure<PickId>(DraftPartErrors.CommunityPicksExceeded);
      }
    }

    if (IsMovieAlreadyPickedInThisPart(movie.Id))
    {
      return Result.Failure<PickId>(DraftPartErrors.MovieAlreadyPickedInThisDraft(movie.Id));
    }

    if (isMovieAlreadyPickedInWholeDraft is not null && isMovieAlreadyPickedInWholeDraft(movie.Id))
    {
      return Result.Failure<PickId>(DraftPartErrors.MovieAlreadyPickedInThisDraft(movie.Id));
    }

    string? effectiveVersionName = movieVersionName;

    if (TryGetRequiredMovieVersion(movie.Id, out var required))
    {
      if (string.IsNullOrWhiteSpace(movieVersionName) ||
        !movieVersionName!.Trim().Equals(required, StringComparison.OrdinalIgnoreCase))
      {
        return Result.Failure<PickId>(MovieErrors.VersionDoesNotMatchRequiredPolicy);
      }

      effectiveVersionName = required;
    }

    var draftPartParticipant = _draftPartParticipants
      .FirstOrDefault(p => p.ParticipantId == participantId);

    if (draftPartParticipant is null)
    {
      return Result.Failure<PickId>(DraftPartErrors.ParticipantNotFound(participantId.Value));
    }

    var pickResult = Pick.Create(
      position: draftPosition,
      movie: movie,
      playedByParticipant: draftPartParticipant,
      draftPart: this,
      movieVersionName: effectiveVersionName,
      playOrder: playOrder);

    if (pickResult.IsFailure)
    {
      return Result.Failure<PickId>(pickResult.Errors);
    }

    var pick = pickResult.Value;

    var addResult = AddPickInternal(pick);

    if (addResult.IsFailure)
    {
      return  Result.Failure<PickId>(addResult.Errors);
    }

    if (participantId.Kind == ParticipantKind.Community)
    {
      IncrementCommunityPicksUsed();
    }

    Raise(new PickAddedDomainEvent(
      draftId: DraftId.Value,
      movie.Id,
      Id.Value,
      participantId.IsDrafter ? participantId.AsDrafterId().Value : Guid.Empty,
      participantId.IsTeam ? participantId.AsDrafterTeamId().Value : Guid.Empty,
      draftPosition,
      playOrder));

    return Result.Success(pick.Id);
  }

  public Result ApplyVeto(
    ISeriesPolicyProvider seriesPolicyProvider,
    SeriesId seriesId,
    DraftType draftType,
    PickId pickId,
    ParticipantId issuerId)
  {
    ArgumentNullException.ThrowIfNull(seriesPolicyProvider);
    ArgumentNullException.ThrowIfNull(seriesId);
    ArgumentNullException.ThrowIfNull(draftType);
    ArgumentNullException.ThrowIfNull(pickId);

    var pick = _picks.FirstOrDefault(p => p.Id.Value == pickId.Value);

    if (pick is null)
    {
      return Result.Failure(DraftPartErrors.PickNotFound(pickId.Value));
    }

    if (Status != DraftPartStatus.InProgress)
    {
      return Result.Failure(DraftPartErrors.DraftNotStarted);
    }

    DraftPartParticipant participant = null!;

    if (issuerId.Kind != ParticipantKind.Community)
    {
      participant = GetParticipantRequired(issuerId);

      if (!participant.CanUseVeto())
      {
        return Result.Failure(DraftPartErrors.NoRemainingVetoes);
      }

      participant.SpendVeto();
    }

    var veto = Veto.Create(pick, participant).Value;

    var apply = pick.ApplyVeto(veto);

    if (apply.IsFailure)
    {
      return apply;
    }

    Raise(new VetoAddedDomainEvent(
      draftId: DraftId.Value,
      participantId: issuerId.Value,
      participantKind: issuerId.Kind.Name,
      pickPosition: pick.Position));

    return Result.Success();
  }


  public Result ApplyVetoOverride(
    ISeriesPolicyProvider seriesPolicyProvider,
    SeriesId seriesId,
    DraftType draftType,
    Veto veto,
    ParticipantId by)
  {
    ArgumentNullException.ThrowIfNull(seriesPolicyProvider);
    ArgumentNullException.ThrowIfNull(seriesId);
    ArgumentNullException.ThrowIfNull(draftType);
    ArgumentNullException.ThrowIfNull(veto);

    if (Status != DraftPartStatus.InProgress)
    {
      return Result.Failure(DraftPartErrors.DraftNotStarted);
    }

    var pick = _picks.FirstOrDefault(p => p.Id == veto.TargetPickId);

    if (pick is null)
    {
      return Result.Failure(DraftPartErrors.PickNotFound(veto.TargetPickId.Value));
    }

    var bugdet = ResolvePartBudget(seriesPolicyProvider, seriesId, draftType);

    if (bugdet.MaxVetoOverrides <= 0)
    {
      return Result.Failure(DraftPartErrors.VetoOverridesNotAllowed);
    }

    var participant = GetParticipantRequired(by);

    if (!participant.CanUseVetoOverride(bugdet.MaxVetoOverrides))
    {
      return Result.Failure(DraftPartErrors.NoRemainingVetoOverrides);
    }

    if (pick.Veto is null || pick.Veto.Id != veto.Id)
    {
      return Result.Failure(DraftPartErrors.VetoNotFound(veto.Id.Value));
    }

    var overrideResults = pick.ApplyVetoOverride(by);

    if (overrideResults.IsFailure)
    {
      return overrideResults;
    }

    participant.SpendVetoOverride(bugdet.MaxVetoOverrides);

    Raise(new VetoOverrideAddedDomainEvent(
      draftId: DraftId.Value,
      participantId: by.Value,
      vetoId: veto.Id.Value));

    return Result.Success();
  }

  public Result ApplyCommissionerOverride(Pick pick)
  {
    ArgumentNullException.ThrowIfNull(pick);

    var overrideEntry = CommissionerOverride.Create(pick);

    if (overrideEntry.IsFailure)
    {
      return overrideEntry;
    }

    var applyResult = pick.ApplyCommissionerOverride(overrideEntry.Value);

    if (applyResult.IsFailure)
    {
      return applyResult;
    }

    var playedBy =  GetParticipantRequired(pick.PlayedByParticipant.ParticipantId);

    playedBy.AddCommissionerOverride();

    return Result.Success();
  }

  public bool TryGetRequiredMovieVersion(Guid movieId, out string versionName)
  {
    versionName = string.Empty;

    if (MovieVersionPolicyType != DraftPartMovieVersionPolicyType.PerMovieRequired)
    {
      return false;
    }

    var match = _requiredMovieVersions.FirstOrDefault(x => x.MovieId == movieId);
    if (match is null)
    {
      return false;
    }

    versionName = match.VersionName;

    return true;
  }

  internal Result SetRequiredMovieVersion(Guid movieId, string versionName)
  {
    if (movieId == Guid.Empty)
    {
      return Result.Failure(MovieErrors.MovieIdRequired);
    }

    if (string.IsNullOrWhiteSpace(versionName))
    {
      return Result.Failure(MovieErrors.VersionIsRequiredByPolicy);
    }

    var trimmed = versionName.Trim();

    if (trimmed.Length > 100)
    {
      return Result.Failure(MovieErrors.VersionNameTooLong);
    }

    MovieVersionPolicyType = DraftPartMovieVersionPolicyType.PerMovieRequired;

    _requiredMovieVersions.RemoveAll(x => x.MovieId == movieId);
    _requiredMovieVersions.Add(new RequiredMovieVersion(movieId, trimmed));

    UpdatedAtUtc = DateTime.UtcNow;
    return Result.Success();
  }

  internal Result SetMovieVersionPolicyNone()
  {
    MovieVersionPolicyType = DraftPartMovieVersionPolicyType.None;
    UpdatedAtUtc = DateTime.UtcNow;
    _requiredMovieVersions.Clear();
    return Result.Success();
  }

  private Result<PickId> AddPickInternal(Pick pick)
  {
    if (_picks.Any(p => p.Position == pick.Position))
    {
      return Result.Failure<PickId>(DraftPartErrors.PickPositionAlreadyExists(pick.Position));
    }

    if (pick.PlayOrder <= 0)
    {
      return Result.Failure<PickId>(DraftPartErrors.InvalidPickPlayOrder(pick.PlayOrder));
    }

    _picks.Add(pick);
    _picks.Sort((a,b) => a.Position.CompareTo(b.Position));
    UpdatedAtUtc = DateTime.UtcNow;

    return Result.Success(pick.Id);
  }

  private bool IsMovieAlreadyPickedInThisPart(Guid movieId) =>
    _picks.Any(p => p.MovieId == movieId && p.IsActiveOnFinalBoard);


  private PartBudget ResolvePartBudget(ISeriesPolicyProvider series, SeriesId seriesId, DraftType draftType)
  {
    var totalParticipants = Participants.Count;
    return series.GetPartBudget(seriesId, draftType,PartIndex, totalParticipants);
  }

  private bool IsParticipantInThisPart(ParticipantId participantId) =>
    _draftPartParticipants.Select(p => p.ParticipantId).Contains(participantId);
}
