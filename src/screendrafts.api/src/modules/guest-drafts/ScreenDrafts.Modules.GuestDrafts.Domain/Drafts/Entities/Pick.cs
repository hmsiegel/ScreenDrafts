using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Errors;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.ValueObjects;

namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Entities;

public sealed class Pick : Entity<PickId>
{
  private readonly List<Veto> _vetoes = [];
  private readonly List<GuestDraftPickEvent> _history = [];

  private Pick(
    DraftId guestDraftId,
    int position,
    int playOrder,
    string moviePublicId,
    Guid movieId,
    DraftParticipant playedByParticipant,
    string? actedByPublicId,
    PickId? id = null
  )
    : base(id ?? PickId.CreateUnique())
  {
    GuestDraftId = guestDraftId;
    Position = position;
    PlayOrder = playOrder;
    MoviePublicId = moviePublicId;
    MovieId = movieId;
    PlayedByParticipant = playedByParticipant;
    PlayedByParticipantId = playedByParticipant.Id;
    ActedByPublicId = actedByPublicId;
  }

  private Pick() { }

  public DraftId GuestDraftId { get; private set; } = default!;

  public int Position { get; private set; }
  public int PlayOrder { get; private set; }

  /// <summary>
  /// Movie's PublicId -- unchanged from before: every domain event, integration
  /// event, and SignalR payload that references a pick's movie keys off this,
  /// not MovieId, so nothing downstream needs to change.
  /// </summary>
  public string MoviePublicId { get; private set; } = default!;

  /// <summary>
  /// FK into GuestDraftMovie (guest_drafts.movies) -- the local movie cache
  /// GuestDrafts now maintains, same pattern as canonical's Movie/Pick.MovieId.
  /// Added purely so GetGuestDraftGameplayQueryHandler can JOIN for
  /// TmdbId/MovieYear/ImdbId instead of round-tripping through IMovieTitleReader.
  /// Resolved once at play-time by PlayPickCommandHandler via
  /// IGuestDraftMovieRepository -- the movie must already be locally cached
  /// (i.e. already imported+synced via MediaAddedIntegrationEvent) before a
  /// pick can reference it, same constraint canonical has.
  /// </summary>
  public Guid MovieId { get; private set; }

  public DraftParticipant PlayedByParticipant { get; private set; } = default!;
  public DraftParticipantId PlayedByParticipantId { get; private set; } = default!;

  /// <summary>
  /// Every guest draft is effectively "hostless" -- guest drafts never have a host
  /// role at all -- so every pick needs a designated revealer. Mirrors canonical
  /// DraftPart.PlayPick's hostless branch: deterministic when there's exactly one
  /// other participant, otherwise resolved by the command handler via a random draw
  /// and passed in explicitly as explicitRevealRecipientId.
  /// </summary>
  public DraftParticipant? RevealAuthorizedParticipant { get; private set; }
  public DraftParticipantId? RevealAuthorizedParticipantId { get; private set; }

  /// <summary>
  /// PublicId of the participant who physically submitted the pick -- may differ
  /// from PlayedByParticipant if, e.g., the owner is submitting on behalf of someone.
  /// </summary>
  public string? ActedByPublicId { get; private set; }

  public IReadOnlyList<Veto> Vetoes => _vetoes.AsReadOnly();

  /// <summary>
  /// The most recent veto on this pick, or null if never vetoed. All veto-state
  /// logic operates against this entry, not the full history -- same reasoning as
  /// canonical Pick.CurrentVeto.
  /// </summary>
  public Veto? CurrentVeto =>
    _vetoes.Count > 0 ? _vetoes.OrderByDescending(v => v.Sequence).First() : null;

  public CommissionerOverride? CommissionerOverride { get; private set; }

  public bool IsActiveOnFinalBoard => !IsVetoed && !IsCommissionerOverridden;
  public bool IsVetoed => CurrentVeto is not null && !CurrentVeto.IsOverridden;
  public bool IsCommissionerOverridden => CommissionerOverride is not null;
  public bool IsEligibleForRePick => IsVetoed && !IsCommissionerOverridden;

  public DateTimeOffset? RevealedAt { get; private set; }
  public bool IsRevealed => RevealedAt.HasValue;

  public IReadOnlyCollection<GuestDraftPickEvent> History => _history.AsReadOnly();

  internal static Result<Pick> Create(
    DraftId guestDraftId,
    int position,
    int playOrder,
    string moviePublicId,
    Guid movieId,
    DraftParticipant playedByParticipant,
    string? actedByPublicId = null,
    PickId? id = null
  )
  {
    ArgumentNullException.ThrowIfNull(playedByParticipant);

    if (position < 1)
    {
      return Result.Failure<Pick>(DraftErrors.PickPositionIsOutOfRange);
    }

    if (playOrder < 1)
    {
      return Result.Failure<Pick>(DraftErrors.InvalidPlayOrder);
    }

    if (string.IsNullOrWhiteSpace(moviePublicId))
    {
      return Result.Failure<Pick>(DraftErrors.MovieMustBeProvided);
    }

    var pick = new Pick(
      guestDraftId: guestDraftId,
      position: position,
      playOrder: playOrder,
      moviePublicId: moviePublicId,
      movieId: movieId,
      playedByParticipant: playedByParticipant,
      actedByPublicId: actedByPublicId,
      id: id
    );

    return pick;
  }

  internal void SetRevealAuthorizedParticipant(DraftParticipant participant)
  {
    RevealAuthorizedParticipant = participant;
    RevealAuthorizedParticipantId = participant.Id;
  }

  public bool IsRevealAuthorized(Guid participantId) =>
    RevealAuthorizedParticipantId is not null
    && RevealAuthorizedParticipantId.Value == participantId;

  internal Result ApplyVeto(Veto veto)
  {
    if (IsVetoed)
    {
      return Result.Failure(DraftErrors.PickAlreadyVetoed);
    }

    _vetoes.Add(veto);
    _history.Add(GuestDraftPickEvent.Veto(veto.IssuedByParticipantId.Value, veto.Note));

    return Result.Success();
  }

  internal Result ApplyCommissionerOverride(CommissionerOverride commissionerOverride)
  {
    ArgumentNullException.ThrowIfNull(commissionerOverride);

    if (CommissionerOverride is not null)
    {
      return Result.Failure(DraftErrors.CommissionerOverrideAlreadyApplied);
    }

    CommissionerOverride = commissionerOverride;
    _history.Add(GuestDraftPickEvent.CommissionerOverride());

    return Result.Success();
  }

  internal Result RevealPick()
  {
    if (IsRevealed)
    {
      return Result.Failure(DraftErrors.PickAlreadyRevealed);
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
      return Result.Failure(DraftErrors.PickNotVetoed);
    }

    if (current.IsOverridden)
    {
      return Result.Failure(DraftErrors.CannotUndoVetoThatHasBeenOverridden);
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
