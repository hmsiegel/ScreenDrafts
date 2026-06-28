namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts;

public sealed partial class DraftPart
{
  private readonly List<CommunityFilmRule> _communityFilmRules = [];

  public IReadOnlyList<CommunityFilmRule> CommunityFilmRules => _communityFilmRules.AsReadOnly();

  public Result AddCommunityFilmRule(
    string publicId,
    CommunityFilmRuleKind ruleKind,
    int? targetSlot
  )
  {
    if (Status != DraftPartStatus.Created)
    {
      return Result.Failure(CommunityFilmRuleErrors.NotModifiable);
    }

    if (ruleKind == CommunityFilmRuleKind.BoostersPick && (targetSlot is null || targetSlot <= 0))
    {
      return Result.Failure(CommunityFilmRuleErrors.TargetSlotRequired);
    }

    var result = CommunityFilmRule.Create(
      ruleKind: ruleKind,
      targetSlot: targetSlot,
      publicId: publicId
    );

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    var rule = result.Value;
    _communityFilmRules.Add(rule);

    EnsureCommunityParticipant();

    UpdatedAtUtc = DateTime.UtcNow;

    return Result.Success();
  }

  public Result UpdateCommunityFilmRule(
    string publicId,
    CommunityFilmRuleKind ruleKind,
    int? targetSlot
  )
  {
    if (Status != DraftPartStatus.Created)
    {
      return Result.Failure(CommunityFilmRuleErrors.NotModifiable);
    }

    if (ruleKind == CommunityFilmRuleKind.BoostersPick && (targetSlot is null || targetSlot <= 0))
    {
      return Result.Failure(CommunityFilmRuleErrors.TargetSlotRequired);
    }

    var rule = _communityFilmRules.FirstOrDefault(r => r.PublicId == publicId);

    if (rule is null)
    {
      return Result.Failure(CommunityFilmRuleErrors.NotFound(publicId));
    }

    rule.Update(
      ruleKind: ruleKind,
      targetSlot: ruleKind == CommunityFilmRuleKind.BoostersPick ? targetSlot : null
    );

    UpdatedAtUtc = DateTime.UtcNow;

    return Result.Success();
  }

  public Result RemoveCommunityFilmRule(string publicId)
  {
    if (Status != DraftPartStatus.Created)
    {
      return Result.Failure(CommunityFilmRuleErrors.NotModifiable);
    }

    var rule = _communityFilmRules.FirstOrDefault(r => r.PublicId == publicId);

    if (rule is null)
    {
      return Result.Failure(CommunityFilmRuleErrors.NotFound(publicId));
    }

    _communityFilmRules.Remove(rule);
    UpdatedAtUtc = DateTime.UtcNow;

    return Result.Success();
  }

  public Result AssignFilmToCommunityFilmRule(string publicId, int tmdbId)
  {
    if (Status != DraftPartStatus.Created)
    {
      return Result.Failure(CommunityFilmRuleErrors.NotModifiable);
    }

    if (_communityFilmRules.Any(r => r.TmdbId == tmdbId && r.PublicId != publicId))
    {
      return Result.Failure(CommunityFilmRuleErrors.FilmAlreadyAssigned(tmdbId));
    }

    var rule = _communityFilmRules.FirstOrDefault(r => r.PublicId == publicId);

    if (rule is null)
    {
      return Result.Failure(CommunityFilmRuleErrors.NotFound(publicId));
    }
    rule.AssignFilm(tmdbId);
    UpdatedAtUtc = DateTime.UtcNow;
    return Result.Success();
  }

  /// <summary>
  /// Checks whether the pick triggers a community film rule auto-veto and,
  /// if so, applies it without spending a veto token.
  ///
  /// BoostersVeto — the film must land at or above TargetSlot (higher slot
  ///   number = higher board position). Auto-veto fires at most once
  ///   (WasAutoVetoFired gate).
  ///
  /// BoostersPick — the film must land at exactly TargetSlot. Auto-veto
  ///   fires every time the film is played at the wrong slot.
  ///
  /// Returns Result.Success() whether or not a veto was applied — no matching
  /// rule is not an error. Returns Result.Failure() only on misconfiguration
  /// (community participant absent) or Veto construction failure.
  /// </summary>
  internal Result TryApplyAutoVeto(Pick pick)
  {
    if (pick.Movie.TmdbId is null)
    {
      return Result.Success();
    }

    var rule = _communityFilmRules.FirstOrDefault(r => r.TmdbId == pick.Movie.TmdbId);

    if (rule is null)
    {
      return Result.Success();
    }

    var shouldVeto = rule.RuleKind switch
    {
      var k when k == CommunityFilmRuleKind.BoostersVeto => !rule.WasAutoVetoFired
        && rule.TargetSlot.HasValue
        && pick.Position > rule.TargetSlot.Value,

      var k when k == CommunityFilmRuleKind.BoostersPick => rule.TargetSlot.HasValue
        && pick.Position != rule.TargetSlot.Value,

      _ => false,
    };

    if (!shouldVeto)
    {
      return Result.Success();
    }

    // AddCommunityFilmRule guarantees this participant exists, but guard
    // defensively in case of legacy data or direct DB manipulation.
    var communityDraftPartParticipant = _draftPartParticipants.FirstOrDefault(p =>
      p.ParticipantIdValue == CommunityParticipants.PatreonMembers.Value
      && p.ParticipantKindValue == ParticipantKind.Community
    );

    if (communityDraftPartParticipant is null)
    {
      return Result.Failure(CommunityFilmRuleErrors.CommunityParticipantNotFound);
    }

    var vetoResult = Veto.Create(
      pick: pick,
      issuedByParticipant: communityDraftPartParticipant,
      actedByPublicId: null,
      note: rule.RuleKind == CommunityFilmRuleKind.BoostersVeto
        ? $"Boosters veto: film must be played at slot {rule.TargetSlot} or higher."
        : $"Boosters pick: film must be played at slot {rule.TargetSlot}."
    );

    if (vetoResult.IsFailure)
    {
      return Result.Failure(vetoResult.Errors);
    }

    var applyResult = pick.ApplyVeto(vetoResult.Value);

    if (applyResult.IsFailure)
    {
      return applyResult;
    }

    if (rule.RuleKind == CommunityFilmRuleKind.BoostersVeto)
    {
      rule.MarkAutoVetoFired();
    }

    Raise(
      new VetoAddedDomainEvent(
        draftPartId: Id.Value,
        draftPartPublicId: PublicId,
        tmdbId: pick.Movie.TmdbId,
        participantId: communityDraftPartParticipant.ParticipantIdValue,
        participantKind: communityDraftPartParticipant.ParticipantKindValue.Value,
        draftId: DraftId.Value,
        draftPublicId: DraftPublicId,
        playOrder: pick.PlayOrder,
        movieTitle: pick.Movie.MovieTitle,
        playedByParticipantId: pick.PlayedByParticipant.ParticipantIdValue,
        playedByParticipantKind: pick.PlayedByParticipant.ParticipantKindValue.Value,
        moviePublicId: pick.Movie.PublicId,
        boardPosition: pick.Position
      )
    );

    Raise(
      new CommunityRuleVetoAppliedDomainEvent(
        draftPartId: Id.Value,
        draftPartPublicId: PublicId,
        draftId: DraftId.Value,
        draftPublicId: DraftPublicId,
        tmdbId: pick.Movie.TmdbId!.Value,
        playOrder: pick.PlayOrder,
        movieTitle: pick.Movie.MovieTitle,
        boardPosition: pick.Position,
        ruleKind: rule.RuleKind.Value,
        targetSlot: rule.TargetSlot.GetValueOrDefault()
      )
    );

    UpdatedAtUtc = DateTime.UtcNow;

    return Result.Success();
  }

  /// <summary>
  /// Ensures CommunityParticipants.PatreonMembers is in _draftPartParticipants.
  /// Called automatically by AddCommunityFilmRule so the veto issuer FK is
  /// always satisfiable. Does not raise a domain event — the participant is
  /// a system actor, not a drafter.
  /// </summary>
  private void EnsureCommunityParticipant()
  {
    var alreadyPresent = _draftPartParticipants.Any(p =>
      p.ParticipantIdValue == CommunityParticipants.PatreonMembers.Value
      && p.ParticipantKindValue == ParticipantKind.Community
    );

    if (alreadyPresent)
    {
      return;
    }

    _draftPartParticipants.Add(
      DraftPartParticipant.Create(this, CommunityParticipants.PatreonMembers)
    );
  }

  /// <summary>
  /// Ensures the community participant has a draft position on the game board
  /// owning the given slot. If a community position already exists, adds the
  /// slot to it. If not, creates one and assigns it to PatreonMembers.
  /// Called only for BoostersPick rules.
  /// </summary>
  private void EnsureCommunityPosition(int slot, string communityPositionPublicId)
  {
    ArgumentNullException.ThrowIfNull(communityPositionPublicId);

    if (GameBoard is null)
    {
      throw new ScreenDraftsException(
        "GameBoard is null. EnsureCommunityPosition must be called after the draft part is added to a game board."
      );
    }

    var existingCommunityPosition = GameBoard.DraftPositions.FirstOrDefault(p =>
      p.AssignedToId == CommunityParticipants.PatreonMembers.Value
    );

    if (existingCommunityPosition is not null)
    {
      existingCommunityPosition.AddSlot(slot);
      return;
    }

    var positionResult = DraftPosition.Create(
      gameBoard: GameBoard,
      name: "Community",
      picks: [slot],
      publicId: communityPositionPublicId
    );

    if (positionResult.IsFailure)
      return;

    var position = positionResult.Value;

    // Assign directly — bypasses AssignParticipant's "already assigned" guard
    // since this is a fresh position.
    position.AssignParticipant(CommunityParticipants.PatreonMembers);

    GameBoard.AddCommunityPosition(position);
  }

  /// <summary>
  /// Creates or updates the community draft position on the game board
  /// based on all current BoostersPick rules. Must be called after
  /// AssignDraftPositions so ClearPositions doesn't remove it.
  /// </summary>
  public Result EnsureCommunityPositions(Func<string> publicIdFactory)
  {
    ArgumentNullException.ThrowIfNull(publicIdFactory);

    var boostersPickRules = _communityFilmRules
      .Where(r => r.RuleKind == CommunityFilmRuleKind.BoostersPick && r.TargetSlot.HasValue)
      .ToList();

    if (boostersPickRules.Count == 0)
    {
      return Result.Success();
    }

    if (GameBoard is null)
    {
      return Result.Failure(DraftPartErrors.GameBoardNotFound);
    }

    // All BoostersPick slots go on one community position.
    // Generate a public ID only if the position doesn't exist yet.
    var existingCommunityPosition = GameBoard.DraftPositions.FirstOrDefault(p =>
      p.AssignedToId == CommunityParticipants.PatreonMembers.Value
    );

    for (var i = 0; i < boostersPickRules.Count; i++)
    {
      var rule = boostersPickRules[i];
      if (existingCommunityPosition is not null)
      {
        existingCommunityPosition.AddSlot(rule.TargetSlot!.Value);
      }
      else
      {
        EnsureCommunityPosition(rule.TargetSlot!.Value, publicIdFactory());
        // After first creation, subsequent rules add slots to the same position.
        existingCommunityPosition = GameBoard.DraftPositions.FirstOrDefault(p =>
          p.AssignedToId == CommunityParticipants.PatreonMembers.Value
        );
      }
    }

    return Result.Success();
  }
}
