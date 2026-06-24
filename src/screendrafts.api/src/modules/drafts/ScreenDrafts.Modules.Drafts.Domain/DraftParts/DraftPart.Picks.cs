namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts;

public sealed partial class DraftPart
{
  public Result<PickId> PlayPick(
    Movie movie,
    int draftPosition,
    int playOrder,
    Participant participantId,
    int canonicalPolicyValue,
    SubDraftId? subDraftId = null,
    string? movieVersionName = null,
    string? actedByPublicId = null,
    Func<Guid, bool>? isMovieAlreadyPickedInWholeDraft = null
  )
  {
    ArgumentNullException.ThrowIfNull(movie);

    if (Status != DraftPartStatus.InProgress)
    {
      return Result.Failure<PickId>(DraftPartErrors.DraftNotStarted);
    }

    if (participantId.Kind == ParticipantKind.Community)
    {
      var budget = ResolvePartBudget(DraftType);

      if (budget.MaxCommunityPicks <= 0)
      {
        return Result.Failure<PickId>(DraftPartErrors.CommunityPicksNotAllowed);
      }

      if (CommunityPicksUsed >= budget.MaxCommunityPicks)
      {
        return Result.Failure<PickId>(DraftPartErrors.CommunityPicksExceeded);
      }
    }

    if (IsMovieAlreadyPickedInThisPart(movie.Id, subDraftId))
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
      if (
        string.IsNullOrWhiteSpace(movieVersionName)
        || !movieVersionName!.Trim().Equals(required, StringComparison.OrdinalIgnoreCase)
      )
      {
        return Result.Failure<PickId>(MovieErrors.VersionDoesNotMatchRequiredPolicy);
      }

      effectiveVersionName = required;
    }

    var draftPartParticipant = _draftPartParticipants.FirstOrDefault(p =>
      p.ParticipantId == participantId
    );

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
      playOrder: playOrder,
      subDraftId: subDraftId,
      actedByPublicId: actedByPublicId
    );

    if (pickResult.IsFailure)
    {
      return Result.Failure<PickId>(pickResult.Errors);
    }

    var pick = pickResult.Value;

    var addResult = AddPickInternal(pick);

    if (addResult.IsFailure)
    {
      return Result.Failure<PickId>(addResult.Errors);
    }

    var autoVetoResult = TryApplyAutoVeto(pick);

    if (autoVetoResult.IsFailure)
    {
      return Result.Failure<PickId>(autoVetoResult.Errors);
    }

    if (DraftType == DraftType.SpeedDraft)
    {
      var revealResult = pick.RevealPick(actedByPublicId);

      if (revealResult.IsFailure)
      {
        return Result.Failure<PickId>(revealResult.Errors);
      }
    }

    if (participantId.Kind == ParticipantKind.Community)
    {
      IncrementCommunityPicksUsed();
    }

    Raise(
      new PickAddedDomainEvent(
        draftPartId: Id.Value,
        draftPartPublicId: PublicId,
        pickId: pick.Id.Value,
        playOrder: playOrder,
        imdbId: movie.ImdbId,
        tmdbId: movie.TmdbId,
        movieTitle: movie.MovieTitle,
        boardPosition: draftPosition,
        moviePublicId: movie.PublicId,
        participantId: participantId.Value,
        participantKind: participantId.Kind.Value,
        draftId: DraftId.Value,
        draftPublicId: DraftPublicId,
        canonicalPolicyValue: canonicalPolicyValue
      )
    );

    return Result.Success(pick.Id);
  }

  public Result UndoPick(int playOrder, SubDraftId? subDraftId = null)
  {
    var pick = _picks.FirstOrDefault(p => p.PlayOrder == playOrder && p.SubDraftId == subDraftId);

    if (pick is null)
    {
      return Result.Success();
    }

    _picks.Remove(pick);

    Raise(
      new PickUndoDomainEvent(
        DraftPartId: Id.Value,
        DraftPartPublicId: PublicId,
        PlayOrder: playOrder,
        BoardPosition: pick.Position,
        TmdbId: pick.Movie.TmdbId ?? 0,
        MovieTitle: pick.Movie.MovieTitle,
        DraftId: DraftId.Value,
        DraftPublicId: DraftPublicId
      )
    );

    UpdatedAtUtc = DateTime.UtcNow;

    return Result.Success();
  }

  public Result RevealPick(int playOrder, string? actedByPublicId)
  {
    if (Status != DraftPartStatus.InProgress)
    {
      return Result.Failure(DraftPartErrors.DraftNotStarted);
    }

    var pick = _picks.SingleOrDefault(p => p.PlayOrder == playOrder);

    if (pick is null)
    {
      return Result.Failure(DraftPartErrors.PickNotFound(playOrder));
    }

    var result = pick.RevealPick(actedByPublicId);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    Raise(
      new PickRevealedDomainEvent(
        Id.Value,
        PublicId,
        pick.Id.Value,
        playOrder,
        pick.MovieId,
        actedByPublicId
      )
    );

    return Result.Success();
  }

  public Result ApplyVeto(PickId pickId, Participant issuerId, string? actedByPublicId = null)
  {
    ArgumentNullException.ThrowIfNull(pickId);

    var pick = _picks.FirstOrDefault(p => p.Id.Value == pickId.Value);

    if (pick is null)
    {
      return Result.Failure(DraftPartErrors.PickNotFound(pickId.Value));
    }

    // ── Veto scope guard ──────────────────────────────────────────────────────
    // A veto may only be applied to the most recently played pick.
    // A pick is "most recent" by play order regardless of veto state —
    // only the last play attempt is eligible.
    var maxPlayOrder = _picks.Max(p => p.PlayOrder);
    if (pick.PlayOrder != maxPlayOrder)
    {
      return Result.Failure(DraftPartErrors.VetoNotOnMostRecentPick);
    }
    // ─────────────────────────────────────────────────────────────────────────

    if (Status != DraftPartStatus.InProgress)
    {
      return Result.Failure(DraftPartErrors.DraftNotStarted);
    }

    var participant = GetParticipantRequired(issuerId);

    if (issuerId.Kind != ParticipantKind.Community)
    {
      if (!participant.CanUseVeto())
      {
        return Result.Failure(DraftPartErrors.NoRemainingVetoes);
      }

      participant.SpendVeto();
    }

    var vetoResult = Veto.Create(
      pick: pick,
      issuedByParticipant: participant,
      actedByPublicId: actedByPublicId
    );

    if (vetoResult.IsFailure)
    {
      return Result.Failure(vetoResult.Errors);
    }

    var veto = vetoResult.Value;

    var apply = pick.ApplyVeto(veto);

    if (apply.IsFailure)
    {
      return apply;
    }

    Raise(
      new VetoAddedDomainEvent(
        draftPartId: Id.Value,
        draftPartPublicId: PublicId,
        tmdbId: pick.Movie.TmdbId,
        participantId: participant.Id.Value,
        participantKind: participant.ParticipantKindValue.Value,
        draftId: DraftId.Value,
        draftPublicId: DraftPublicId,
        playOrder: pick.PlayOrder,
        movieTitle: pick.Movie?.MovieTitle,
        playedByParticipantId: pick.PlayedByParticipant.ParticipantIdValue,
        playedByParticipantKind: pick.PlayedByParticipant.ParticipantKindValue.Value,
        moviePublicId: pick.Movie!.PublicId,
        boardPosition: pick.Position
      )
    );
    return Result.Success();
  }

  /// <summary>
  /// Reverses a veto by play order. Restores the pick to the board and
  /// refunds the veto token to the original issuer.
  /// Commissioner-only / break-glass operation.
  /// </summary>
  public Result UndoVeto(int playOrder)
  {
    if (Status != DraftPartStatus.InProgress)
    {
      return Result.Failure(DraftPartErrors.DraftNotStarted);
    }

    var pick = _picks.FirstOrDefault(p => p.PlayOrder == playOrder);

    if (pick is null)
    {
      return Result.Failure(DraftPartErrors.PickNotFound(playOrder));
    }

    if (pick.Veto is null)
    {
      return Result.Failure(PickErrors.PickNotVetoed);
    }

    // Refund the veto token to the issuer before clearing the veto
    var issuerParticipant = _draftPartParticipants.FirstOrDefault(p =>
      p.Id == pick.Veto.IssuedByParticipantId
    );

    if (issuerParticipant is not null)
    {
      issuerParticipant.RefundVeto();
    }

    var result = pick.UndoVeto();

    if (result.IsFailure)
    {
      return result;
    }

    Raise(
      new VetoUndoDomainEvent(
        DraftPartId: Id.Value,
        DraftPartPublicId: PublicId,
        PickId: pick.Id.Value,
        PlayOrder: playOrder,
        TmdbId: pick.Movie.TmdbId ?? 0,
        DraftId: DraftId.Value,
        DraftPublicId: DraftPublicId,
        movieTitle: pick.Movie?.MovieTitle
      )
    );

    UpdatedAtUtc = DateTime.UtcNow;

    return Result.Success();
  }

  public Result ApplyVetoOverride(
    int playOrder,
    Participant by,
    int canonicalPolicyValue,
    string? actedByPublicId = null
  )
  {
    if (DraftType == DraftType.SpeedDraft || DraftType == DraftType.Standard)
    {
      return Result.Failure(DraftPartErrors.VetoOverridesNotAllowedInSpeedDrafts);
    }

    if (Status != DraftPartStatus.InProgress)
    {
      return Result.Failure(DraftPartErrors.DraftNotStarted);
    }

    var pick = _picks.FirstOrDefault(p => p.PlayOrder == playOrder);

    if (pick is null)
    {
      return Result.Failure(DraftPartErrors.PickNotFound(playOrder));
    }

    if (pick.Veto is null)
    {
      return Result.Failure(DraftPartErrors.VetoNotFound(playOrder));
    }

    var overrideResults = pick.Veto.Override(by, actedByPublicId);

    if (overrideResults.IsFailure)
    {
      return overrideResults;
    }

    if (pick.Movie?.TmdbId is not null)
    {
      Raise(
        new VetoOverrideAddedDomainEvent(
          draftPartId: Id.Value,
          draftPartPublicId: PublicId,
          tmdbId: pick.Movie.TmdbId!.Value,
          participantId: pick.PlayedByParticipant.ParticipantId.Value,
          participantKind: pick.PlayedByParticipant.ParticipantKindValue.Value,
          draftId: DraftId.Value,
          draftPublicId: DraftPublicId,
          canonicalPolicyValue: canonicalPolicyValue,
          playOrder: pick.PlayOrder,
          movieTitle: pick.Movie?.MovieTitle
        )
      );
    }

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

    var playedBy = GetParticipantRequired(pick.PlayedByParticipant.ParticipantId);

    playedBy.AddCommissionerOverride();

    Raise(
      new CommissionerOverrideAppliedDomainEvent(
        draftPartId: Id.Value,
        draftPartPublicId: PublicId,
        tmdbId: pick.Movie.TmdbId ?? 0,
        participantId: pick.PlayedByParticipant.ParticipantId.Value,
        draftId: DraftId.Value,
        draftPublicId: DraftPublicId,
        participantKind: pick.PlayedByParticipant.ParticipantKindValue.Value,
        moviePublicId: pick.Movie!.PublicId,
        movieTitle: pick.Movie!.MovieTitle,
        boardPosition: pick.Position
      )
    );
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

  private Result<PickId> AddPickInternal(Pick pick, SubDraftId? subDraftId = null)
  {
    // Block duplicate position only if the existing pick at that slot landed.
    // A vetoed pick that is eligible for re-pick does not block the slot.
    var landedAtPosition = _picks.Any(p =>
      p.Position == pick.Position && p.SubDraftId == subDraftId && !p.IsEligibleForRePick
    );

    if (landedAtPosition)
    {
      return Result.Failure<PickId>(DraftPartErrors.PickPositionAlreadyExists(pick.Position));
    }

    if (pick.PlayOrder <= 0)
    {
      return Result.Failure<PickId>(DraftPartErrors.InvalidPickPlayOrder(pick.PlayOrder));
    }

    _picks.Add(pick);
    _picks.Sort((a, b) => a.Position.CompareTo(b.Position));
    UpdatedAtUtc = DateTime.UtcNow;

    return Result.Success(pick.Id);
  }

  private bool IsMovieAlreadyPickedInThisPart(Guid movieId, SubDraftId? subDraftId = null) =>
    _picks.Any(p => p.MovieId == movieId && p.SubDraftId == subDraftId && !p.IsEligibleForRePick);

  private PartBudget ResolvePartBudget(DraftType draftType)
  {
    var budget = SeriesPolicyRules.ComputePartBudget(draftType, Participants.Count);

    if (MaxCommunityPicks > 0 || MaxCommunityVetoes > 0)
    {
      return budget with
      {
        MaxCommunityPicks = MaxCommunityPicks,
        MaxCommunityVetoes = MaxCommunityVetoes,
      };
    }

    return budget;
  }

  private bool IsParticipantInThisPart(Participant participantId) =>
    _draftPartParticipants.Select(p => p.ParticipantId).Contains(participantId);
}
