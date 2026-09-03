using OpenTelemetry.Trace;
using ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.DomainEvents;

namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts;

public sealed class GuestDraft : Entity<GuestDraftId>
{
  public const int TitleMaxLength = 150;

  private readonly List<GuestDraftParticipant> _participants = [];
  private readonly List<GuestDraftPick> _picks = [];

  private GuestDraft(
    string publicId,
    Guid ownerUserId,
    string title,
    GuestDraftType guestDraftType,
    DateTime createdOnUtc,
    GuestDraftId? id = null
  )
    : base(id ?? GuestDraftId.CreateUnique())
  {
    PublicId = publicId;
    OwnerUserId = ownerUserId;
    Title = title;
    GuestDraftType = guestDraftType;
    GuestDraftStatus = GuestDraftStatus.Created;
    CreatedOnUtc = createdOnUtc;
  }

  private GuestDraft() { }

  public string PublicId { get; private set; } = default!;
  public Guid OwnerUserId { get; private set; }
  public string Title { get; private set; } = default!;
  public GuestDraftType GuestDraftType { get; private set; } = default!;
  public GuestDraftStatus GuestDraftStatus { get; private set; } = default!;

  // Not populated in this pass -- sharing (read-only link access) is a follow-up
  // feature. Column/config included now so the migration doesn't need to change
  // shape later.
  public string? ShareToken { get; private set; } = default!;

  public DateTime CreatedOnUtc { get; private set; }
  public DateTime? UpdatedOnUtc { get; private set; }

  public GuestDraftGameBoard? GameBoard { get; private set; }

  public IReadOnlyCollection<GuestDraftParticipant> Participants => _participants.AsReadOnly();
  public IReadOnlyCollection<GuestDraftPick> Picks => _picks.AsReadOnly();

  public static Result<GuestDraft> Create(
    string publicId,
    Guid ownerUserId,
    string ownerParticipantPublicId,
    string title,
    GuestDraftType guestDraftType
  )
  {
    if (string.IsNullOrWhiteSpace(title))
    {
      return Result.Failure<GuestDraft>(GuestDraftErrors.TitleIsRequired);
    }

    var guestDraft = new GuestDraft(
      publicId: publicId,
      ownerUserId: ownerUserId,
      title: title,
      guestDraftType: guestDraftType,
      createdOnUtc: DateTime.UtcNow
    );

    guestDraft._participants.Add(
      GuestDraftParticipant.Create(
        publicId: ownerParticipantPublicId,
        guestDraftId: guestDraft.Id,
        userId: ownerUserId,
        isOwner: true
      )
    );

    return Result.Success(guestDraft);
  }

  public Result<GuestDraftParticipant> InviteParticipant(string participantPublicId, Guid userId)
  {
    if (GuestDraftStatus != GuestDraftStatus.Created)
    {
      return Result.Failure<GuestDraftParticipant>(GuestDraftErrors.CannotInviteAfterStart);
    }

    if (_participants.Any(p => p.UserId == userId))
    {
      return Result.Failure<GuestDraftParticipant>(
        GuestDraftErrors.ParticipantAlreadyAdded(userId)
      );
    }

    var participant = GuestDraftParticipant.Create(
      publicId: participantPublicId,
      guestDraftId: Id,
      userId: userId,
      isOwner: false
    );

    _participants.Add(participant);
    UpdatedOnUtc = DateTime.UtcNow;

    return Result.Success(participant);
  }

  // ── Participant lookup ───────────────────────────────────────────────────

  public bool HasParticipant(Guid participantId) =>
    _participants.Any(p => p.Id.Value == participantId);

  public GuestDraftParticipant? FindParticipant(Guid participantId) =>
    _participants.FirstOrDefault(p => p.Id.Value == participantId);

  internal GuestDraftParticipant GetParticipantRequired(Guid participantId) =>
    FindParticipant(participantId)
    ?? throw new ArgumentException(
      $"Participant not found: {participantId}",
      nameof(participantId)
    );

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

    if (GuestDraftStatus != GuestDraftStatus.Created)
    {
      return Result.Failure(GuestDraftErrors.CannotChangeBoardAfterStart);
    }

    var template = GuestDraftBoardTemplates.GetFixedTemplate(GuestDraftType);

    if (template is null)
    {
      return Result.Failure(GuestDraftErrors.DraftTypeDoesNotHaveAFixedLayout(GuestDraftType.Name));
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

    if (GuestDraftStatus != GuestDraftStatus.Created)
    {
      return Result.Failure(GuestDraftErrors.CannotChangeBoardAfterStart);
    }

    if (GuestDraftBoardTemplates.IsFixed(GuestDraftType))
    {
      return Result.Failure(GuestDraftErrors.DraftTypeHasAFixedLayout(GuestDraftType.Name));
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
    GameBoard ??= GuestDraftGameBoard.Create(Id);

    var created = new List<GuestDraftPosition>();

    foreach (var p in positions)
    {
      var positionResult = GuestDraftPosition.Create(
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

    var assignResult = GameBoard.AssignPositions(created, _participants.Count);

    if (assignResult.IsFailure)
    {
      return assignResult;
    }

    UpdatedOnUtc = DateTime.UtcNow;
    return Result.Success();
  }

  public Result AssignParticipantToPosition(GuestDraftPosition position, Guid participantId)
  {
    ArgumentNullException.ThrowIfNull(position);

    if (GameBoard is null || position.GameBoardId != GameBoard.Id)
    {
      return Result.Failure(GuestDraftErrors.PositionDoesNotBelongToThisBoard);
    }

    if (!HasParticipant(participantId))
    {
      return Result.Failure(GuestDraftErrors.ParticipantNotFound(participantId));
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
    if (GuestDraftStatus != GuestDraftStatus.Created)
    {
      return Result.Failure(GuestDraftErrors.DraftCanOnlyBeStartedIfCreated);
    }

    if (_participants.Count < 2)
    {
      return Result.Failure(GuestDraftErrors.CannotStartWithoutAtLeastTwoParticipants);
    }

    if (GameBoard is null || GameBoard.Positions.Count != _participants.Count)
    {
      return Result.Failure(GuestDraftErrors.BoardMustBeFullySetUpBeforeStarting);
    }

    if (GameBoard.Positions.Any(p => p.AssignedToParticipantId is null))
    {
      return Result.Failure(GuestDraftErrors.AllPositionsMustBeAssignedBeforeStarting);
    }

    foreach (var participant in _participants)
    {
      participant.InitializeVetoes(startingVetoes: 1);
    }

    GuestDraftStatus = GuestDraftStatus.InProgress;
    UpdatedOnUtc = DateTime.UtcNow;

    Raise(
      new GuestDraftStartedDomainEvent(
        guestDraftId: Id.Value,
        guestDraftPublicId: PublicId,
        participantCount: _participants.Count
      )
    );

    return Result.Success();
  }

  public Result Complete()
  {
    if (GuestDraftStatus != GuestDraftStatus.InProgress)
    {
      return Result.Failure(GuestDraftErrors.CannotCompleteIfNotInProgress);
    }

    var totalPicks = GameBoard?.Positions.SelectMany(p => p.Picks).Count() ?? 0;
    var landedCount = _picks
      .Where(p => p.IsActiveOnFinalBoard)
      .Select(p => p.Position)
      .Distinct()
      .Count();

    if (landedCount != totalPicks)
    {
      return Result.Failure(GuestDraftErrors.CannotCompleteWithoutAllPicks);
    }

    GuestDraftStatus = GuestDraftStatus.Completed;
    UpdatedOnUtc = DateTime.UtcNow;

    var vetoCount = _picks.Sum(p => p.Vetoes.Count);

    Raise(
      new GuestDraftCompletedDomainEvent(
        guestDraftId: Id.Value,
        guestDraftPublicId: PublicId,
        totalPicks: totalPicks,
        vetoCount: vetoCount
      )
    );

    return Result.Success();
  }

  // ── Gameplay ─────────────────────────────────────────────────────────────

  public Result<GuestDraftPickId> PlayPick(
    string moviePublicId,
    int position,
    int playOrder,
    Guid participantId,
    string? actedByPublicId = null,
    GuestDraftParticipantId? explicitRevealRecipientId = null
  )
  {
    if (GuestDraftStatus != GuestDraftStatus.InProgress)
    {
      return Result.Failure<GuestDraftPickId>(GuestDraftErrors.DraftNotStarted);
    }

    if (IsMovieAlreadyPicked(moviePublicId))
    {
      return Result.Failure<GuestDraftPickId>(GuestDraftErrors.MovieAlreadyPicked);
    }

    var participant = FindParticipant(participantId);

    if (participant is null)
    {
      return Result.Failure<GuestDraftPickId>(GuestDraftErrors.ParticipantNotFound(participantId));
    }

    var pickResult = GuestDraftPick.Create(
      guestDraftId: Id,
      position: position,
      playOrder: playOrder,
      moviePublicId: moviePublicId,
      playedByParticipant: participant,
      actedByPublicId: actedByPublicId
    );

    if (pickResult.IsFailure)
    {
      return Result.Failure<GuestDraftPickId>(pickResult.Errors);
    }

    var pick = pickResult.Value;

    var addResult = AddPickInternal(pick);

    if (addResult.IsFailure)
    {
      return Result.Failure<GuestDraftPickId>(addResult.Errors);
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
      new GuestDraftPickPlayedDomainEvent(
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
      new GuestDraftPickUndoneDomainEvent(
        guestDraftId: Id.Value,
        guestDraftPublicId: PublicId,
        playOrder: playOrder,
        boardPosition: pick.Position,
        moviePublicId: pick.MoviePublicId
      )
    );

    return Result.Success();
  }

  public Result RevealPick(GuestDraftPickId pickId)
  {
    ArgumentNullException.ThrowIfNull(pickId);
    if (GuestDraftStatus != GuestDraftStatus.InProgress)
    {
      return Result.Failure(GuestDraftErrors.DraftNotStarted);
    }

    var pick = _picks.FirstOrDefault(p => p.Id == pickId);

    if (pick is null)
    {
      return Result.Failure(GuestDraftErrors.PickNotFound(pickId.Value));
    }

    var result = pick.RevealPick();

    if (result.IsFailure)
    {
      return result;
    }

    Raise(
      new GuestDraftPickRevealedDomainEvent(
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
    GuestDraftPickId pickId,
    Guid issuerParticipantId,
    string? actedByPublicId = null,
    string? note = null
  )
  {
    ArgumentNullException.ThrowIfNull(pickId);

    var pick = _picks.FirstOrDefault(p => p.Id == pickId);

    if (pick is null)
    {
      return Result.Failure(GuestDraftErrors.PickNotFound(pickId.Value));
    }

    var maxPlayOrder = _picks.Max(p => p.PlayOrder);

    if (pick.PlayOrder != maxPlayOrder)
    {
      return Result.Failure(GuestDraftErrors.VetoNotOnMostRecentPick);
    }

    if (GuestDraftStatus != GuestDraftStatus.InProgress)
    {
      return Result.Failure(GuestDraftErrors.DraftNotStarted);
    }

    var participant = GetParticipantRequired(issuerParticipantId);

    if (!participant.CanUseVeto())
    {
      return Result.Failure(GuestDraftErrors.NoRemainingVetoes);
    }

    var spentFromFungiblePool = participant.SpendVeto();

    var vetoResult = GuestDraftVeto.Create(
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
      new GuestDraftVetoAppliedDomainEvent(
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
  public Result UndoVeto(GuestDraftPickId pickId)
  {
    ArgumentNullException.ThrowIfNull(pickId);

    if (GuestDraftStatus != GuestDraftStatus.InProgress)
    {
      return Result.Failure(GuestDraftErrors.DraftNotStarted);
    }

    var pick = _picks.FirstOrDefault(p => p.Id == pickId);

    if (pick is null)
    {
      return Result.Failure(GuestDraftErrors.PickNotFound(pickId.Value));
    }

    var currentVeto = pick.CurrentVeto;

    if (currentVeto is null)
    {
      return Result.Failure(GuestDraftErrors.PickNotVetoed);
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
      new GuestDraftVetoUndoneDomainEvent(
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
    GuestDraftPickId pickId,
    Guid byParticipantId,
    string? actedByPublicId = null,
    string? note = null
  )
  {
    ArgumentNullException.ThrowIfNull(pickId);

    if (GuestDraftType == GuestDraftType.Standard)
    {
      return Result.Failure(GuestDraftErrors.VetoOverridesNotAllowedForThisDraftType);
    }

    if (GuestDraftStatus != GuestDraftStatus.InProgress)
    {
      return Result.Failure(GuestDraftErrors.DraftNotStarted);
    }

    var pick = _picks.FirstOrDefault(p => p.Id == pickId);

    if (pick is null)
    {
      return Result.Failure(GuestDraftErrors.PickNotFound(pickId.Value));
    }

    if (pick.CurrentVeto is null)
    {
      return Result.Failure(GuestDraftErrors.VetoNotFound(pickId.Value));
    }

    if (pick.PlayedByParticipantId.Value == byParticipantId)
    {
      return Result.Failure(GuestDraftErrors.CannotOverrideOwnPick);
    }

    var participant = GetParticipantRequired(byParticipantId);
    var maxOverrides = 1; // placeholder -- see note above

    if (!participant.CanUseVetoOverride(maxOverrides))
    {
      return Result.Failure(GuestDraftErrors.NoRemainingVetoOverrides);
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
      new GuestDraftVetoOverriddenDomainEvent(
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

  public Result ApplyCommissionerOverride(GuestDraftPickId pickId)
  {
    ArgumentNullException.ThrowIfNull(pickId);

    var pick = _picks.FirstOrDefault(p => p.Id == pickId);

    if (pick is null)
    {
      return Result.Failure(GuestDraftErrors.PickNotFound(pickId.Value));
    }

    var maxPlayOrder = _picks.Max(p => p.PlayOrder);

    if (pick.PlayOrder != maxPlayOrder)
    {
      return Result.Failure(GuestDraftErrors.CommissionerOverrideNotOnMostRecentPick);
    }

    if (GuestDraftStatus != GuestDraftStatus.InProgress)
    {
      return Result.Failure(GuestDraftErrors.DraftNotStarted);
    }

    var overrideResult = GuestDraftCommissionerOverride.Create(pick);

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
      new GuestDraftCommissionerOverrideAppliedDomainEvent(
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

  private Result AddPickInternal(GuestDraftPick pick)
  {
    // Block duplicate position only if the existing pick at that slot landed. A
    // vetoed pick that is eligible for re-pick does not block the slot.
    var landedAtPosition = _picks.Any(p => p.Position == pick.Position && p.IsActiveOnFinalBoard);

    if (landedAtPosition)
    {
      return Result.Failure(GuestDraftErrors.PickPositionAlreadyExists(pick.Position));
    }

    if (pick.PlayOrder <= 0)
    {
      return Result.Failure(GuestDraftErrors.InvalidPlayOrder);
    }

    _picks.Add(pick);
    _picks.Sort((a, b) => a.Position.CompareTo(b.Position));

    return Result.Success();
  }

  private bool IsMovieAlreadyPicked(string moviePublicId) =>
    _picks.Any(p => p.MoviePublicId == moviePublicId && !p.IsEligibleForRePick);
}
