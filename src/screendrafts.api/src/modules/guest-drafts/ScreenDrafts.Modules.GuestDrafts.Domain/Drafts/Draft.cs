namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts;

public sealed class Draft : Entity<DraftId>
{
  public const int TitleMaxLength = 150;

  private readonly List<DraftParticipant> _participants = [];
  private readonly List<Pick> _picks = [];

  private Draft(
    string publicId,
    Guid ownerUserId,
    string title,
    DraftType guestDraftType,
    DateTime createdOnUtc,
    DateOnly? dateOnly = null,
    DraftId? id = null
  )
    : base(id ?? DraftId.CreateUnique())
  {
    PublicId = publicId;
    OwnerUserId = ownerUserId;
    Title = title;
    GuestDraftType = guestDraftType;
    GuestDraftStatus = DraftStatus.Created;
    DraftDate = dateOnly;
    CreatedOnUtc = createdOnUtc;
  }

  private Draft() { }

  public string PublicId { get; private set; } = default!;
  public Guid OwnerUserId { get; private set; }
  public string Title { get; private set; } = default!;
  public DraftType GuestDraftType { get; private set; } = default!;
  public DraftStatus GuestDraftStatus { get; private set; } = default!;

  // Not populated in this pass -- sharing (read-only link access) is a follow-up
  // feature. Column/config included now so the migration doesn't need to change
  // shape later.
  public string? ShareToken { get; private set; } = default!;

  public DateTime CreatedOnUtc { get; private set; }
  public DateTime? UpdatedOnUtc { get; private set; }
  public DateOnly? DraftDate { get; private set; }

  public GameBoard? GameBoard { get; private set; }

  public IReadOnlyCollection<DraftParticipant> Participants => _participants.AsReadOnly();
  public IReadOnlyCollection<Pick> Picks => _picks.AsReadOnly();

  public static Result<Draft> Create(
    string publicId,
    Guid ownerUserId,
    string title,
    DraftType guestDraftType,
    DateOnly? draftDate = null
  )
  {
    if (string.IsNullOrWhiteSpace(title))
    {
      return Result.Failure<Draft>(DraftErrors.TitleIsRequired);
    }

    var guestDraft = new Draft(
      publicId: publicId,
      ownerUserId: ownerUserId,
      title: title,
      guestDraftType: guestDraftType,
      createdOnUtc: DateTime.UtcNow,
      dateOnly: draftDate
    );

    return Result.Success(guestDraft);
  }

  public Result SetDraftDate(DateOnly? draftDate)
  {
    DraftDate = draftDate;
    UpdatedOnUtc = DateTime.UtcNow;
    return Result.Success();
  }

  public Result SetTitle(string title)
  {
    if (string.IsNullOrWhiteSpace(title))
    {
      return Result.Failure(DraftErrors.TitleIsRequired);
    }

    Title = title;
    UpdatedOnUtc = DateTime.UtcNow;
    return Result.Success();
  }

  public Result ChangeType(DraftType newType)
  {
    if (GuestDraftStatus != DraftStatus.Created)
    {
      return Result.Failure(DraftErrors.CannotChangeDraftTypeAfterStart);
    }

    if (GuestDraftType == newType)
    {
      return Result.Success();
    }

    ClearBoard();
    GuestDraftType = newType;
    UpdatedOnUtc = DateTime.UtcNow;

    return Result.Success();
  }

  private void ClearBoard()
  {
    if (GameBoard is null)
    {
      return;
    }

    foreach (var position in GameBoard.Positions)
    {
      if (position.AssignedToParticipantId is not { } assignedId)
      {
        continue;
      }

      var participant = FindParticipant(assignedId);

      if (participant is null)
      {
        continue;
      }

      if (position.HasBonusVeto)
      {
        participant.RevokeAward(isVeto: true);
      }

      if (position.HasBonusVetoOverride)
      {
        participant.RevokeAward(isVeto: false);
      }

      if (position.HasBonusFungibleToken)
      {
        participant.RevokeFungibleTokenAward();
      }
    }

    GameBoard = null;
  }

  public Result<DraftParticipant> AddParticipant(Participant participant, bool isOwner)
  {
    if (GuestDraftStatus != DraftStatus.Created)
    {
      return Result.Failure<DraftParticipant>(DraftErrors.CannotAddParticipantAfterStart);
    }

    if (_participants.Any(p => p.ParticipantId == participant))
    {
      return Result.Failure<DraftParticipant>(
        DraftErrors.ParticipantAlreadyAdded(participant.Value)
      );
    }

    var newParticipant = DraftParticipant.Create(
      guestDraftId: Id,
      participantId: participant,
      isOwner: isOwner
    );

    _participants.Add(newParticipant);
    UpdatedOnUtc = DateTime.UtcNow;

    return Result.Success(newParticipant);
  }

  // ── Participant lookup ───────────────────────────────────────────────────

  public bool HasParticipant(Guid participantId) =>
    _participants.Any(p => p.Id.Value == participantId);

  public DraftParticipant? FindParticipant(Guid participantId) =>
    _participants.FirstOrDefault(p => p.Id.Value == participantId);

  internal DraftParticipant GetParticipantRequired(Guid participantId) =>
    FindParticipant(participantId)
    ?? throw new ArgumentException(
      $"Participant not found: {participantId}",
      nameof(participantId)
    );

  public DraftParticipant? FindByParticipantRef(Participant participant) =>
    _participants.FirstOrDefault(p => p.ParticipantId == participant);

  // ── Board setup ──────────────────────────────────────────────────────────

  /// <summary>
  /// Applies the fixed board layout for Standard/MiniSuper guest drafts -- mirrors
  /// positions-editor.tsx's getDefaultPositions exactly (7/6/4/2 vs 5/3/1 for
  /// Standard; 5/3/1 vs 4/2 for MiniSuper). Fails for any other GuestDraftType; use
  /// SetCustomPositions for those instead.
  /// </summary>
  public Result UseFixedBoardLayout(Func<string, string> positionPublicIdGenerator)
  {
    ArgumentNullException.ThrowIfNull(positionPublicIdGenerator);

    if (GuestDraftStatus != DraftStatus.Created)
    {
      return Result.Failure(DraftErrors.CannotChangeBoardAfterStart);
    }

    var template = GameBoardTemplates.GetFixedTemplate(GuestDraftType);

    if (template is null)
    {
      return Result.Failure(DraftErrors.DraftTypeDoesNotHaveAFixedLayout(GuestDraftType.Name));
    }

    var positions = template
      .Select(t =>
        (t.Name, t.Picks, t.HasBonusVeto, t.HasBonusVetoOverride, t.HasBonusFungibleToken)
      )
      .ToList();

    return SetPositions(positions, positionPublicIdGenerator);
  }

  /// <summary>
  /// Applies a custom board layout for MiniMega/Super/Mega guest drafts -- the owner
  /// supplies each position's name, pick slots, and bonus flags directly, mirroring
  /// the owner-supplied seeds pattern in canonical's SetSpeedDraftPositions.
  /// </summary>
  public Result SetCustomPositions(
    IReadOnlyList<(
      string Name,
      IReadOnlyList<int> Picks,
      bool HasBonusVeto,
      bool HasBonusVetoOverride,
      bool HasBonusFungibleToken
    )> positions,
    Func<string, string> positionPublicIdGenerator
  )
  {
    ArgumentNullException.ThrowIfNull(positions);
    ArgumentNullException.ThrowIfNull(positionPublicIdGenerator);

    if (GuestDraftStatus != DraftStatus.Created)
    {
      return Result.Failure(DraftErrors.CannotChangeBoardAfterStart);
    }

    if (GameBoardTemplates.IsFixed(GuestDraftType))
    {
      return Result.Failure(DraftErrors.DraftTypeHasAFixedLayout(GuestDraftType.Name));
    }

    return SetPositions(positions, positionPublicIdGenerator);
  }

  private Result SetPositions(
    IReadOnlyList<(
      string Name,
      IReadOnlyList<int> Picks,
      bool HasBonusVeto,
      bool HasBonusVetoOverride,
      bool HasBonusFungibleToken
    )> positions,
    Func<string, string> positionPublicIdGenerator
  )
  {
    GameBoard ??= GameBoard.Create(Id);

    var created = new List<DraftPosition>();

    foreach (var p in positions)
    {
      var positionResult = DraftPosition.Create(
        gameBoardId: GameBoard.Id,
        publicId: positionPublicIdGenerator(p.Name),
        name: p.Name,
        picks: p.Picks,
        hasBonusVeto: p.HasBonusVeto,
        hasBonusVetoOverride: p.HasBonusVetoOverride,
        hasBonusFungibleToken: p.HasBonusFungibleToken
      );

      if (positionResult.IsFailure)
      {
        return Result.Failure(positionResult.Errors);
      }

      created.Add(positionResult.Value);
    }

    var assignResult = GameBoard.AssignPositions(created);

    if (assignResult.IsFailure)
    {
      return assignResult;
    }

    UpdatedOnUtc = DateTime.UtcNow;
    return Result.Success();
  }

  public Result AssignParticipantToPosition(DraftPosition position, Guid participantId)
  {
    ArgumentNullException.ThrowIfNull(position);

    if (GameBoard is null || position.GameBoardId != GameBoard.Id)
    {
      return Result.Failure(DraftErrors.PositionDoesNotBelongToThisBoard);
    }

    if (!HasParticipant(participantId))
    {
      return Result.Failure(DraftErrors.ParticipantNotFound(participantId));
    }

    var assignResult = position.AssignParticipant(participantId);

    if (assignResult.IsFailure)
    {
      return assignResult;
    }

    var participant = GetParticipantRequired(participantId);

    if (position.HasBonusVeto)
    {
      participant.GrantAward(isVeto: true);
    }

    if (position.HasBonusVetoOverride)
    {
      participant.GrantAward(isVeto: false);
    }

    if (position.HasBonusFungibleToken)
    {
      participant.GrantFungibleTokenAward();
    }

    UpdatedOnUtc = DateTime.UtcNow;
    return Result.Success();
  }

  // ── Lifecycle ────────────────────────────────────────────────────────────

  public Result Start()
  {
    if (GuestDraftStatus != DraftStatus.Created)
    {
      return Result.Failure(DraftErrors.DraftCanOnlyBeStartedIfCreated);
    }

    if (_participants.Count < 2)
    {
      return Result.Failure(DraftErrors.CannotStartWithoutAtLeastTwoParticipants);
    }

    if (GameBoard is null || GameBoard.Positions.Count != _participants.Count)
    {
      return Result.Failure(DraftErrors.BoardMustBeFullySetUpBeforeStarting);
    }

    if (GameBoard.Positions.Any(p => p.AssignedToParticipantId is null))
    {
      return Result.Failure(DraftErrors.AllPositionsMustBeAssignedBeforeStarting);
    }

    foreach (var participant in _participants)
    {
      participant.InitializeVetoes(startingVetoes: 1);
    }

    GuestDraftStatus = DraftStatus.InProgress;
    UpdatedOnUtc = DateTime.UtcNow;

    Raise(
      new DraftStartedDomainEvent(
        draftId: Id.Value,
        draftPublicId: PublicId,
        participantCount: _participants.Count
      )
    );

    return Result.Success();
  }

  public Result Complete()
  {
    if (GuestDraftStatus != DraftStatus.InProgress)
    {
      return Result.Failure(DraftErrors.CannotCompleteIfNotInProgress);
    }

    var totalPicks = GameBoard?.Positions.SelectMany(p => p.Picks).Count() ?? 0;
    var landedCount = _picks
      .Where(p => p.IsActiveOnFinalBoard)
      .Select(p => p.Position)
      .Distinct()
      .Count();

    if (landedCount != totalPicks)
    {
      return Result.Failure(DraftErrors.CannotCompleteWithoutAllPicks);
    }

    GuestDraftStatus = DraftStatus.Completed;
    UpdatedOnUtc = DateTime.UtcNow;

    var vetoCount = _picks.Sum(p => p.Vetoes.Count);

    Raise(
      new DraftCompletedDomainEvent(
        draftId: Id.Value,
        draftPublicId: PublicId,
        totalPicks: totalPicks,
        vetoCount: vetoCount
      )
    );

    return Result.Success();
  }

  // ── Gameplay ─────────────────────────────────────────────────────────────

  public Result<PickId> PlayPick(
    string moviePublicId,
    Guid movieId,
    int position,
    int playOrder,
    Guid participantId,
    string? actedByPublicId = null,
    DraftParticipantId? explicitRevealRecipientId = null
  )
  {
    if (GuestDraftStatus != DraftStatus.InProgress)
    {
      return Result.Failure<PickId>(DraftErrors.DraftNotStarted);
    }

    if (IsMovieAlreadyPicked(moviePublicId))
    {
      return Result.Failure<PickId>(DraftErrors.MovieAlreadyPicked);
    }

    var participant = FindParticipant(participantId);

    if (participant is null)
    {
      return Result.Failure<PickId>(DraftErrors.ParticipantNotFound(participantId));
    }

    var pickResult = Pick.Create(
      guestDraftId: Id,
      position: position,
      playOrder: playOrder,
      moviePublicId: moviePublicId,
      movieId: movieId,
      playedByParticipant: participant,
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

    // Every guest draft is hostless -- always needs a designated revealer.
    var recipient = explicitRevealRecipientId is not null
      ? FindParticipant(explicitRevealRecipientId.Value)
      : null;

    if (recipient is null)
    {
      var others = _participants.Where(p => p.Id != participant.Id).ToList();

      if (others.Count == 1)
      {
        recipient = others[0];
      }
    }

    if (recipient is not null)
    {
      pick.SetRevealAuthorizedParticipant(recipient);
    }

    Raise(
      new PickPlayedDomainEvent(
        guestDraftId: Id.Value,
        guestDraftPublicId: PublicId,
        pickId: pick.Id.Value,
        playOrder: pick.PlayOrder,
        boardPosition: pick.Position,
        moviePublicId: pick.MoviePublicId,
        playedByParticipantId: participant.Id.Value,
        actedByPublicId: actedByPublicId,
        revealAuthorizedParticipantId: recipient?.Id.Value
      )
    );

    UpdatedOnUtc = DateTime.UtcNow;

    return Result.Success(pick.Id);
  }

  public Result UndoPick(int playOrder)
  {
    var pick = _picks.FirstOrDefault(p => p.PlayOrder == playOrder);

    if (pick is null)
    {
      return Result.Success();
    }

    _picks.Remove(pick);
    UpdatedOnUtc = DateTime.UtcNow;

    Raise(
      new PickUndoneDomainEvent(
        guestDraftId: Id.Value,
        guestDraftPublicId: PublicId,
        playOrder: playOrder,
        boardPosition: pick.Position,
        moviePublicId: pick.MoviePublicId
      )
    );

    return Result.Success();
  }

  public Result RevealPick(PickId pickId)
  {
    ArgumentNullException.ThrowIfNull(pickId);
    if (GuestDraftStatus != DraftStatus.InProgress)
    {
      return Result.Failure(DraftErrors.DraftNotStarted);
    }

    var pick = _picks.FirstOrDefault(p => p.Id == pickId);

    if (pick is null)
    {
      return Result.Failure(DraftErrors.PickNotFound(pickId.Value));
    }

    var result = pick.RevealPick();

    if (result.IsFailure)
    {
      return result;
    }

    Raise(
      new PickRevealedDomainEvent(
        guestDraftId: Id.Value,
        guestDraftPublicId: PublicId,
        pickId: pick.Id.Value,
        playOrder: pick.PlayOrder,
        boardPosition: pick.Position,
        moviePublicId: pick.MoviePublicId,
        playedByParticipantId: pick.PlayedByParticipantId.Value
      )
    );

    UpdatedOnUtc = DateTime.UtcNow;
    return Result.Success();
  }

  /// <summary>
  /// A veto may only be applied to the most recently played pick, by play order,
  /// regardless of that pick's veto state -- mirrors canonical DraftPart.ApplyVeto's
  /// scope guard exactly.
  /// </summary>
  public Result ApplyVeto(
    PickId pickId,
    Guid issuerParticipantId,
    string? actedByPublicId = null,
    string? note = null
  )
  {
    ArgumentNullException.ThrowIfNull(pickId);

    var pick = _picks.FirstOrDefault(p => p.Id == pickId);

    if (pick is null)
    {
      return Result.Failure(DraftErrors.PickNotFound(pickId.Value));
    }

    var maxPlayOrder = _picks.Max(p => p.PlayOrder);

    if (pick.PlayOrder != maxPlayOrder)
    {
      return Result.Failure(DraftErrors.VetoNotOnMostRecentPick);
    }

    if (GuestDraftStatus != DraftStatus.InProgress)
    {
      return Result.Failure(DraftErrors.DraftNotStarted);
    }

    var participant = GetParticipantRequired(issuerParticipantId);

    if (!participant.CanUseVeto())
    {
      return Result.Failure(DraftErrors.NoRemainingVetoes);
    }

    var spentFromFungiblePool = participant.SpendVeto();

    var vetoResult = Veto.Create(
      pick: pick,
      issuedByParticipant: participant,
      actedByPublicId: actedByPublicId,
      note: spentFromFungiblePool ? note : null,
      spentFromFungiblePool: spentFromFungiblePool
    );

    if (vetoResult.IsFailure)
    {
      participant.RefundVeto(spentFromFungiblePool);
      return Result.Failure(vetoResult.Errors);
    }

    var apply = pick.ApplyVeto(vetoResult.Value);

    if (apply.IsFailure)
    {
      participant.RefundVeto(spentFromFungiblePool);
      return apply;
    }

    Raise(
      new VetoAppliedDomainEvent(
        guestDraftId: Id.Value,
        guestDraftPublicId: PublicId,
        pickId: pick.Id.Value,
        playOrder: pick.PlayOrder,
        moviePublicId: pick.MoviePublicId,
        vetoedByParticipantId: participant.Id.Value,
        playedByParticipantId: pick.PlayedByParticipantId.Value,
        vetoTokensRemaining: participant.TotalVetoes - participant.VetoesUsed,
        overrideTokensRemaining: participant.TotalVetoOverrides - participant.VetoOverridesUsed
      )
    );

    UpdatedOnUtc = DateTime.UtcNow;
    return Result.Success();
  }

  /// <summary>
  /// Reverses a veto by pick id. Commissioner-only / break-glass operation --
  /// mirrors canonical DraftPart.UndoVeto.
  /// </summary>
  public Result UndoVeto(PickId pickId)
  {
    ArgumentNullException.ThrowIfNull(pickId);

    if (GuestDraftStatus != DraftStatus.InProgress)
    {
      return Result.Failure(DraftErrors.DraftNotStarted);
    }

    var pick = _picks.FirstOrDefault(p => p.Id == pickId);

    if (pick is null)
    {
      return Result.Failure(DraftErrors.PickNotFound(pickId.Value));
    }

    var currentVeto = pick.CurrentVeto;

    if (currentVeto is null)
    {
      return Result.Failure(DraftErrors.PickNotVetoed);
    }

    // Capture before mutating -- pick.UndoVeto() below removes the veto from
    // pick.Vetoes, so CurrentVeto would no longer be available afterward.
    var issuerParticipantId = currentVeto.IssuedByParticipantId.Value;
    var spentFromFungiblePool = currentVeto.SpentFromFungiblePool;

    var result = pick.UndoVeto();

    if (result.IsFailure)
    {
      return result;
    }

    var issuer = FindParticipant(issuerParticipantId);
    issuer?.RefundVeto(spentFromFungiblePool);

    Raise(
      new VetoUndoneDomainEvent(
        guestDraftId: Id.Value,
        guestDraftPublicId: PublicId,
        pickId: pick.Id.Value,
        playOrder: pick.PlayOrder,
        moviePublicId: pick.MoviePublicId,
        refundedToParticipantId: issuer?.Id.Value,
        vetoTokensRemaining: issuer is null ? null : issuer.TotalVetoes - issuer.VetoesUsed,
        overrideTokensRemaining: issuer is null
          ? null
          : issuer.TotalVetoOverrides - issuer.VetoOverridesUsed
      )
    );

    UpdatedOnUtc = DateTime.UtcNow;
    return Result.Success();
  }

  // NOTE: maxOverrides below is a placeholder (1) -- canonical sources the real
  // per-draft-type cap from SeriesPolicyRules.ComputePartBudget, which I don't
  // have visibility into. Only the "Standard blocks overrides entirely" rule is
  // confirmed (mirrors ApplyVetoOverride's DraftType == Standard/SpeedDraft guard,
  // minus SpeedDraft since it isn't a GuestDraftType). Confirm real numbers for
  // MiniMega/MiniSuper/Super/Mega before this ships.
  public Result ApplyVetoOverride(
    PickId pickId,
    Guid byParticipantId,
    string? actedByPublicId = null,
    string? note = null
  )
  {
    ArgumentNullException.ThrowIfNull(pickId);

    if (GuestDraftType == DraftType.Standard)
    {
      return Result.Failure(DraftErrors.VetoOverridesNotAllowedForThisDraftType);
    }

    if (GuestDraftStatus != DraftStatus.InProgress)
    {
      return Result.Failure(DraftErrors.DraftNotStarted);
    }

    var pick = _picks.FirstOrDefault(p => p.Id == pickId);

    if (pick is null)
    {
      return Result.Failure(DraftErrors.PickNotFound(pickId.Value));
    }

    if (pick.CurrentVeto is null)
    {
      return Result.Failure(DraftErrors.VetoNotFound(pickId.Value));
    }

    if (pick.PlayedByParticipantId.Value == byParticipantId)
    {
      return Result.Failure(DraftErrors.CannotOverrideOwnPick);
    }

    var participant = GetParticipantRequired(byParticipantId);
    var maxOverrides = 1; // placeholder -- see note above

    if (!participant.CanUseVetoOverride(maxOverrides))
    {
      return Result.Failure(DraftErrors.NoRemainingVetoOverrides);
    }

    var spentFromFungiblePool = participant.SpendVetoOverride(maxOverrides);

    var overrideResult = pick.CurrentVeto.Override(
      by: participant,
      actedByPublicId: actedByPublicId,
      note: spentFromFungiblePool ? note : null,
      spentFromFungiblePool: spentFromFungiblePool
    );

    if (overrideResult.IsFailure)
    {
      participant.RefundVetoOverride(spentFromFungiblePool);
      return overrideResult;
    }

    Raise(
      new VetoOverriddenDomainEvent(
        guestDraftId: Id.Value,
        guestDraftPublicId: PublicId,
        pickId: pick.Id.Value,
        playOrder: pick.PlayOrder,
        moviePublicId: pick.MoviePublicId,
        overriddenByParticipantId: participant.Id.Value,
        vetoTokensRemaining: participant.TotalVetoes - participant.VetoesUsed,
        overrideTokensRemaining: participant.TotalVetoOverrides - participant.VetoOverridesUsed
      )
    );

    UpdatedOnUtc = DateTime.UtcNow;
    return Result.Success();
  }

  public Result ApplyCommissionerOverride(PickId pickId)
  {
    ArgumentNullException.ThrowIfNull(pickId);

    var pick = _picks.FirstOrDefault(p => p.Id == pickId);

    if (pick is null)
    {
      return Result.Failure(DraftErrors.PickNotFound(pickId.Value));
    }

    var maxPlayOrder = _picks.Max(p => p.PlayOrder);

    if (pick.PlayOrder != maxPlayOrder)
    {
      return Result.Failure(DraftErrors.CommissionerOverrideNotOnMostRecentPick);
    }

    if (GuestDraftStatus != DraftStatus.InProgress)
    {
      return Result.Failure(DraftErrors.DraftNotStarted);
    }

    var overrideResult = CommissionerOverride.Create(pick);

    if (overrideResult.IsFailure)
    {
      return Result.Failure(overrideResult.Errors);
    }

    var applyResult = pick.ApplyCommissionerOverride(overrideResult.Value);

    if (applyResult.IsFailure)
    {
      return applyResult;
    }

    var playedBy = GetParticipantRequired(pick.PlayedByParticipantId.Value);
    playedBy.AddCommissionerOverride();

    Raise(
      new CommissionerOverrideAppliedDomainEvent(
        guestDraftId: Id.Value,
        guestDraftPublicId: PublicId,
        pickId: pick.Id.Value,
        playOrder: pick.PlayOrder,
        boardPosition: pick.Position,
        moviePublicId: pick.MoviePublicId,
        playedByParticipantId: playedBy.Id.Value,
        vetoTokensRemaining: playedBy.TotalVetoes - playedBy.VetoesUsed,
        overrideTokensRemaining: playedBy.TotalVetoOverrides - playedBy.VetoOverridesUsed
      )
    );

    UpdatedOnUtc = DateTime.UtcNow;
    return Result.Success();
  }

  // ── Private helpers ──────────────────────────────────────────────────────

  private Result AddPickInternal(Pick pick)
  {
    // Block duplicate position only if the existing pick at that slot landed. A
    // vetoed pick that is eligible for re-pick does not block the slot.
    var landedAtPosition = _picks.Any(p => p.Position == pick.Position && p.IsActiveOnFinalBoard);

    if (landedAtPosition)
    {
      return Result.Failure(DraftErrors.PickPositionAlreadyExists(pick.Position));
    }

    if (pick.PlayOrder <= 0)
    {
      return Result.Failure(DraftErrors.InvalidPlayOrder);
    }

    _picks.Add(pick);
    _picks.Sort((a, b) => a.Position.CompareTo(b.Position));

    return Result.Success();
  }

  private bool IsMovieAlreadyPicked(string moviePublicId) =>
    _picks.Any(p => p.MoviePublicId == moviePublicId && !p.IsEligibleForRePick);
}
