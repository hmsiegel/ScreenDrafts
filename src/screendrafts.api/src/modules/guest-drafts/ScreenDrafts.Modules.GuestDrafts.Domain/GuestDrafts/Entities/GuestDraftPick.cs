namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.Entities;

public sealed class GuestDraftPick : Entity<GuestDraftPickId>
{
  private readonly List<GuestDraftVeto> _vetoes = [];
  private readonly List<GuestDraftPickEvent> _history = [];

  private GuestDraftPick(
    GuestDraftId guestDraftId,
    int position,
    int playOrder,
    string moviePublicId,
    GuestDraftParticipant playedByParticipant,
    string? actedByPublicId,
    GuestDraftPickId? id = null
  )
    : base(id ?? GuestDraftPickId.CreateUnique())
  {
    GuestDraftId = guestDraftId;
    Position = position;
    PlayOrder = playOrder;
    MoviePublicId = moviePublicId;
    PlayedByParticipant = playedByParticipant;
    PlayedByParticipantId = playedByParticipant.Id;
    ActedByPublicId = actedByPublicId;
  }

  private GuestDraftPick() { }

  public GuestDraftId GuestDraftId { get; private set; } = default!;

  public int Position { get; private set; }
  public int PlayOrder { get; private set; }

  /// <summary>
  /// Movie referenced by public id only -- no local Movie cache, unlike canonical
  /// Drafts. Resolved live via IMovieTitleReader by whatever reads this pick, per
  /// the "use the existing public APIs" decision.
  /// </summary>
  public string MoviePublicId { get; private set; } = default!;

  public GuestDraftParticipant PlayedByParticipant { get; private set; } = default!;
  public GuestDraftParticipantId PlayedByParticipantId { get; private set; } = default!;

  /// <summary>
  /// Every guest draft is effectively "hostless" -- guest drafts never have a host
  /// role at all -- so every pick needs a designated revealer. Mirrors canonical
  /// DraftPart.PlayPick's hostless branch: deterministic when there's exactly one
  /// other participant, otherwise resolved by the command handler via a random draw
  /// and passed in explicitly as explicitRevealRecipientId.
  /// </summary>
  public GuestDraftParticipant? RevealAuthorizedParticipant { get; private set; }
  public GuestDraftParticipantId? RevealAuthorizedParticipantId { get; private set; }

  /// <summary>
  /// PublicId of the participant who physically submitted the pick -- may differ
  /// from PlayedByParticipant if, e.g., the owner is submitting on behalf of someone.
  /// </summary>
  public string? ActedByPublicId { get; private set; }

  public IReadOnlyList<GuestDraftVeto> Vetoes => _vetoes.AsReadOnly();

  /// <summary>
  /// The most recent veto on this pick, or null if never vetoed. All veto-state
  /// logic operates against this entry, not the full history -- same reasoning as
  /// canonical Pick.CurrentVeto.
  /// </summary>
  public GuestDraftVeto? CurrentVeto =>
    _vetoes.Count > 0 ? _vetoes.OrderByDescending(v => v.Sequence).First() : null;

  public GuestDraftCommissionerOverride? CommissionerOverride { get; private set; }

  public bool IsActiveOnFinalBoard => !IsVetoed && !IsCommissionerOverridden;
  public bool IsVetoed => CurrentVeto is not null && !CurrentVeto.IsOverridden;
  public bool IsCommissionerOverridden => CommissionerOverride is not null;
  public bool IsEligibleForRePick => IsVetoed && !IsCommissionerOverridden;

  public DateTimeOffset? RevealedAt { get; private set; }
  public bool IsRevealed => RevealedAt.HasValue;

  public IReadOnlyCollection<GuestDraftPickEvent> History => _history.AsReadOnly();

  internal static Result<GuestDraftPick> Create(
    GuestDraftId guestDraftId,
    int position,
    int playOrder,
    string moviePublicId,
    GuestDraftParticipant playedByParticipant,
    string? actedByPublicId = null,
    GuestDraftPickId? id = null
  )
  {
    ArgumentNullException.ThrowIfNull(playedByParticipant);

    if (position < 1)
    {
      return Result.Failure<GuestDraftPick>(GuestDraftErrors.PickPositionIsOutOfRange);
    }

    if (playOrder < 1)
    {
      return Result.Failure<GuestDraftPick>(GuestDraftErrors.InvalidPlayOrder);
    }

    if (string.IsNullOrWhiteSpace(moviePublicId))
    {
      return Result.Failure<GuestDraftPick>(GuestDraftErrors.MovieMustBeProvided);
    }

    var pick = new GuestDraftPick(
      guestDraftId: guestDraftId,
      position: position,
      playOrder: playOrder,
      moviePublicId: moviePublicId,
      playedByParticipant: playedByParticipant,
      actedByPublicId: actedByPublicId,
      id: id
    );

    return pick;
  }

  internal void SetRevealAuthorizedParticipant(GuestDraftParticipant participant)
  {
    RevealAuthorizedParticipant = participant;
    RevealAuthorizedParticipantId = participant.Id;
  }

  public bool IsRevealAuthorized(Guid participantId) =>
    RevealAuthorizedParticipantId is not null
    && RevealAuthorizedParticipantId.Value == participantId;

  internal Result ApplyVeto(GuestDraftVeto veto)
  {
    if (IsVetoed)
    {
      return Result.Failure(GuestDraftErrors.PickAlreadyVetoed);
    }

    _vetoes.Add(veto);
    _history.Add(GuestDraftPickEvent.Veto(veto.IssuedByParticipantId.Value, veto.Note));

    return Result.Success();
  }

  internal Result ApplyCommissionerOverride(GuestDraftCommissionerOverride commissionerOverride)
  {
    ArgumentNullException.ThrowIfNull(commissionerOverride);

    if (CommissionerOverride is not null)
    {
      return Result.Failure(GuestDraftErrors.CommissionerOverrideAlreadyApplied);
    }

    CommissionerOverride = commissionerOverride;
    _history.Add(GuestDraftPickEvent.CommissionerOverride());

    return Result.Success();
  }

  internal Result RevealPick()
  {
    if (IsRevealed)
    {
      return Result.Failure(GuestDraftErrors.PickAlreadyRevealed);
    }

    RevealedAt = DateTimeOffset.UtcNow;
    _history.Add(GuestDraftPickEvent.Revealed());

    return Result.Success();
  }

  /// <summary>
  /// Reverses a veto on this pick. Commissioner-only / break-glass operation --
  /// mirrors canonical Pick.UndoVeto.
  /// </summary>
  internal Result UndoVeto()
  {
    var current = CurrentVeto;

    if (current is null)
    {
      return Result.Failure(GuestDraftErrors.PickNotVetoed);
    }

    if (current.IsOverridden)
    {
      return Result.Failure(GuestDraftErrors.CannotUndoVetoThatHasBeenOverridden);
    }

    _vetoes.Remove(current);
    _history.Add(GuestDraftPickEvent.Undo());

    return Result.Success();
  }
}

public sealed record GuestDraftPickEvent(
  string Kind,
  Guid? IssuerParticipantId,
  string? Note,
  DateTime OccurredOnUtc
)
{
  public static GuestDraftPickEvent Played() => new("Played", null, null, DateTime.UtcNow);

  public static GuestDraftPickEvent Veto(Guid issuerParticipantId, string? note) =>
    new("Veto", issuerParticipantId, note, DateTime.UtcNow);

  public static GuestDraftPickEvent VetoOverride(Guid issuerParticipantId) =>
    new("VetoOverride", issuerParticipantId, null, DateTime.UtcNow);

  public static GuestDraftPickEvent CommissionerOverride() =>
    new("CommissionerOverride", null, null, DateTime.UtcNow);

  public static GuestDraftPickEvent Revealed() => new("Revealed", null, null, DateTime.UtcNow);

  public static GuestDraftPickEvent Undo() => new("Undo", null, null, DateTime.UtcNow);
}
