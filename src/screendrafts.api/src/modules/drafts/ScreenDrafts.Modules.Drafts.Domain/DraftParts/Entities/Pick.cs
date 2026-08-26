namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;

public sealed class Pick : Entity<PickId>
{
  private readonly List<PickEvent> _history = [];
  private readonly List<Veto> _vetoes = [];
  private readonly List<TeamPickCredit> _teamPickCredits = [];

  private Pick(
    int position,
    Movie movie,
    DraftPart draftPart,
    DraftPartParticipant playedByParticipant,
    string actedByPublicId,
    int playOrder = 0,
    SubDraftId? subDraftId = null,
    PickId? id = null
  )
    : base(id ?? PickId.CreateUnique())
  {
    Position = position;
    PlayOrder = playOrder;

    SubDraftId = subDraftId;

    Movie = Guard.Against.Null(movie);
    MovieId = movie.Id;

    DraftPart = Guard.Against.Null(draftPart);
    DraftPartId = draftPart.Id;

    PlayedByParticipant = playedByParticipant;
    PlayedByParticipantId = playedByParticipant.Id;

    PlayedByParticipantIdValue = playedByParticipant.ParticipantIdValue;
    PlayedByParticipantKindValue = playedByParticipant.ParticipantKindValue;

    ActedByPublicId = actedByPublicId;
  }

  private Pick() { }

  public int Position { get; private set; }
  public int PlayOrder { get; private set; }

  public Guid MovieId { get; private set; }
  public Movie Movie { get; private set; } = default!;

  public string? MovieVersionName { get; private set; } = default!;

  public DraftPartId DraftPartId { get; private set; } = default!;
  public DraftPart DraftPart { get; private set; } = default!;

  public DraftPartParticipant PlayedByParticipant { get; private set; } = default!;
  public DraftPartParticipantId PlayedByParticipantId { get; private set; } = default!;

  public Guid PlayedByParticipantIdValue { get; private set; }
  public ParticipantKind PlayedByParticipantKindValue { get; private set; } = default!;

  /// <summary>
  /// Only set on picks in a hostless DraftPart (Draft.IsHostless) — the participant who
  /// "received" this pick and is therefore the one authorized to reveal it, standing in
  /// for host authority which doesn't exist in these drafts. Derived deterministically
  /// ("the other drafter") when the part has exactly 2 drafters; otherwise resolved by
  /// PlayPickCommandHandler via a random draw (the app performs the equivalent of the
  /// offline random.org process) and passed in. Null for every hosted pick.
  /// </summary>
  public DraftPartParticipant? RevealAuthorizedParticipant { get; private set; }
  public DraftPartParticipantId? RevealAuthorizedParticipantId { get; private set; }
  public Guid? RevealAuthorizedParticipantIdValue { get; private set; }
  public ParticipantKind? RevealAuthorizedParticipantKindValue { get; private set; }

  public SubDraftId? SubDraftId { get; private set; }

  /// <summary>
  /// The public ID of the user who physically submitted the pick.
  /// This may differ from the PlayedByParticipant if, for example, a commissioner is submitting a pick on behalf of a participant.
  /// Null for system-generated community picks.
  /// </summary>
  public string? ActedByPublicId { get; private set; }

  /// <summary>
  /// The full, ordered veto history for this pick. Normally holds at most one entry.
  /// Can hold more than one when a veto is overridden ant the resulting override is
  /// iteself overridden (i.e. the pick is vetoed again). See <see cref="CurrentVeto"/>.
  /// </summary>
  public IReadOnlyList<Veto> Vetoes => _vetoes.AsReadOnly();

  /// <summary>
  /// The most recent veto applied to this pick, or null if the pick has never been vetoed.
  /// All veto-state logic (IsVetoed, ApplyVetoOverride, UndoVeto) operates against this entry,
  /// not the full history.
  /// </summary>
  public Veto? CurrentVeto =>
    _vetoes.Count > 0 ? _vetoes.OrderByDescending(v => v.Sequence).First() : null;

  [NotMapped]
  public VetoId? VetoId => CurrentVeto?.Id;

  /// <summary>
  /// Set only when PlayedByParticipantKindValue is Team — snapshots which individual
  /// drafters were on the team at the moment this pick was created, so each of them gets
  /// personal credit (film history, appearance counts) without the team itself also
  /// accruing a separate stat line. Never recomputed from current team membership; see
  /// TeamPickCredit's remarks for why. Empty for every non-Team pick.
  /// </summary>
  public IReadOnlyList<TeamPickCredit> TeamPickCredits => _teamPickCredits.AsReadOnly();

  public CommissionerOverride? CommissionerOverride { get; private set; } = default!;

  [NotMapped]
  public Guid? CommissionerOverrideId => CommissionerOverride?.Id;

  public bool IsActiveOnFinalBoard => !IsVetoed && !IsCommissionerOverridden;

  [NotMapped]
  public bool IsVetoed => CurrentVeto is not null && !CurrentVeto.IsOverridden;

  [NotMapped]
  public bool IsCommissionerOverridden => CommissionerOverride is not null;

  /// <summary>
  /// A vetoed pick can be re-picked by the same participant or another eligible participant.
  /// A commissioner-overridden pick is permanently removed from the board and cannot be re-picked.
  /// </summary>
  [NotMapped]
  public bool IsEligibleForRePick => IsVetoed && !IsCommissionerOverridden;

  public DateTimeOffset? RevealedAt { get; private set; }

  [NotMapped]
  public bool IsRevealed => RevealedAt.HasValue;

  public IReadOnlyCollection<PickEvent> History => _history.AsReadOnly();

  internal static Result<Pick> Create(
    int position,
    Movie movie,
    DraftPartParticipant playedByParticipant,
    DraftPart draftPart,
    int playOrder,
    string? actedByPublicId = null,
    string? movieVersionName = null,
    MovieVersionPolicy? versionPolicy = null,
    SubDraftId? subDraftId = null,
    PickId? id = null
  )
  {
    if (draftPart is null)
    {
      return Result.Failure<Pick>(PickErrors.DraftMustBeProvided);
    }

    if (
      !draftPart.MinPosition.HasValue
      || !draftPart.MaxPosition.HasValue
      || draftPart.MinPosition.Value <= 0
      || draftPart.MaxPosition.Value <= 0
    )
    {
      return Result.Failure<Pick>(PickErrors.PartPositionsNotSet);
    }

    var minPosition = draftPart.MinPosition.Value;
    var maxPosition = draftPart.MaxPosition.Value;

    if (minPosition > maxPosition)
    {
      return Result.Failure<Pick>(PickErrors.InvalidPartPositionRange);
    }

    if (position < minPosition || position > maxPosition)
    {
      return Result.Failure<Pick>(PickErrors.PickPositionIsOutOfRange);
    }

    if (playOrder < 1)
    {
      return Result.Failure<Pick>(PickErrors.InvalidPlayOrder);
    }

    if (movie is null)
    {
      return Result.Failure<Pick>(PickErrors.MovieMustBeProvided);
    }

    if (!draftPart.HasParticipant(playedByParticipant.ParticipantId))
    {
      return Result.Failure<Pick>(PickErrors.ParticipantNotInDraftPart);
    }

    var pick = new Pick(
      position: position,
      movie: movie,
      playedByParticipant: playedByParticipant,
      draftPart: draftPart,
      actedByPublicId: actedByPublicId ?? string.Empty,
      playOrder: playOrder,
      subDraftId: subDraftId,
      id: id
    );

    var setVersionResult = pick.SetMovieVersionName(
      movieVersionName: movieVersionName,
      movie: movie,
      versionPolicy: versionPolicy
    );

    if (setVersionResult.IsFailure)
    {
      return Result.Failure<Pick>(setVersionResult.Errors);
    }

    return pick;
  }

  internal static Result<Pick> SeedCreate(
    int position,
    Movie movie,
    DraftPart draftPart,
    DraftPartParticipant playedByParticipant,
    int playOrder,
    string? movieVersionName = null,
    PickId? id = null
  )
  {
    if (draftPart is null)
    {
      return Result.Failure<Pick>(PickErrors.DraftMustBeProvided);
    }

    if (!draftPart.MinPosition.HasValue || !draftPart.MaxPosition.HasValue)
    {
      return Result.Failure<Pick>(PickErrors.PartPositionsNotSet);
    }

    var minPosition = draftPart.MinPosition.Value;
    var maxPosition = draftPart.MaxPosition.Value;

    if (position < minPosition || position > maxPosition)
    {
      return Result.Failure<Pick>(PickErrors.PickPositionIsOutOfRange);
    }

    if (playOrder < 1)
    {
      return Result.Failure<Pick>(PickErrors.InvalidPlayOrder);
    }

    if (movie is null)
    {
      return Result.Failure<Pick>(PickErrors.MovieMustBeProvided);
    }

    if (playedByParticipant is null)
    {
      return Result.Failure<Pick>(PickErrors.ParticipantNotInDraftPart);
    }

    if (!draftPart.HasParticipant(playedByParticipant.ParticipantId))
    {
      return Result.Failure<Pick>(PickErrors.ParticipantNotInDraftPart);
    }

    var pick = new Pick(
      position: position,
      movie: movie,
      draftPart: draftPart,
      playedByParticipant: playedByParticipant,
      playOrder: playOrder,
      actedByPublicId: string.Empty,
      id: id
    );

    var versionResult = pick.SetMovieVersionName(movieVersionName, movie, versionPolicy: null);

    if (versionResult.IsFailure)
    {
      return Result.Failure<Pick>(versionResult.Errors);
    }

    return pick;
  }

  internal Result SetMovieVersionName(
    string? movieVersionName,
    Movie movie,
    MovieVersionPolicy? versionPolicy = null
  )
  {
    if (string.IsNullOrWhiteSpace(movieVersionName))
    {
      if (versionPolicy is not null && versionPolicy.RequiresPickLevelVersion)
      {
        return Result.Failure(MovieErrors.VersionIsRequiredByPolicy);
      }

      MovieVersionName = null;
      return Result.Success();
    }

    var trimmed = movieVersionName.Trim();

    if (trimmed.Length > 100)
    {
      return Result.Failure(MovieErrors.VersionNameTooLong);
    }

    if (versionPolicy is not null)
    {
      var policyCheck = versionPolicy.Validate(trimmed);

      if (policyCheck.IsFailure)
      {
        return policyCheck;
      }
    }

    if (movie.TryNormalizeVersionName(trimmed, out var canonical))
    {
      MovieVersionName = canonical;
      return Result.Success();
    }

    if (movie.HasDefinedVersions && (versionPolicy?.AllowsFreeformPickVersion is not true))
    {
      return Result.Failure(MovieErrors.UnknownVersionForMovie);
    }

    MovieVersionName = trimmed;
    return Result.Success();
  }

  /// <summary>
  /// Snapshots individual drafter credit for a Team-played pick. Called once, right after
  /// the pick is created, with whichever drafters are on the team at that instant — see
  /// TeamPickCredit's remarks for why this is a one-time snapshot rather than a live
  /// membership lookup. No-op (and safe to call) with an empty or null list; only
  /// meaningful when PlayedByParticipantKindValue is Team, but doesn't itself enforce that
  /// — the caller (DraftPart.PlayPick) only calls this for Team-kind picks.
  /// </summary>
  internal void SetTeamPickCredits(IReadOnlyCollection<Guid>? drafterIdValues)
  {
    _teamPickCredits.Clear();

    if (drafterIdValues is null || drafterIdValues.Count == 0)
    {
      return;
    }

    foreach (var drafterIdValue in drafterIdValues.Distinct())
    {
      _teamPickCredits.Add(TeamPickCredit.Create(this, drafterIdValue));
    }
  }

  /// <summary>
  /// Sets which participant is authorized to reveal this pick — only meaningful for
  /// hostless drafts. See RevealAuthorizedParticipant's remarks for how the value is chosen.
  /// </summary>
  internal void SetRevealAuthorizedParticipant(DraftPartParticipant participant)
  {
    RevealAuthorizedParticipant = participant;
    RevealAuthorizedParticipantId = participant.Id;
    RevealAuthorizedParticipantIdValue = participant.ParticipantIdValue;
    RevealAuthorizedParticipantKindValue = participant.ParticipantKindValue;
  }

  /// <summary>
  /// True if the given participant is this pick's designated revealer. Only meaningful
  /// when RevealAuthorizedParticipant is set (hostless drafts) — always false otherwise,
  /// which is correct: a hosted pick's reveal authority belongs to the primary host, not
  /// any participant, so RevealPickCommandHandler checks host status directly in that case
  /// and never calls this.
  /// </summary>
  public bool IsRevealAuthorized(Participant participant) =>
    RevealAuthorizedParticipantIdValue == participant.Value
    && RevealAuthorizedParticipantKindValue == participant.Kind;

  internal Result ApplyVeto(Veto veto)
  {
    if (IsVetoed)
    {
      return Result.Failure(PickErrors.PickAlreadyVetoed);
    }

    _vetoes.Add(veto);

    _history.Add(
      PickEvent.Veto(
        issuerId: veto.IssuedByParticipant.ParticipantId,
        actedByPublicId: veto.ActedByPublicId,
        note: veto.Note
      )
    );

    return Result.Success();
  }

  internal Result ApplyCommissionerOverride(CommissionerOverride commissionerOverride)
  {
    Guard.Against.Null(commissionerOverride);

    if (CommissionerOverride is not null)
    {
      return Result.Failure(PickErrors.CommissionerOverrideAlreadyApplied);
    }

    CommissionerOverride = commissionerOverride;

    return Result.Success();
  }

  internal Result ApplyVetoOverride(Participant by, string? actedByPublicId = null)
  {
    if (CurrentVeto is null || !IsVetoed)
    {
      return Result.Failure(PickErrors.CannotOverrideAPickThatHasNotBeenVetoed);
    }

    var result = CurrentVeto.Override(by, actedByPublicId);

    if (result.IsFailure)
    {
      return result;
    }

    _history.Add(PickEvent.VetoOverride(by: by, actedByPublicId: CurrentVeto.ActedByPublicId));

    return Result.Success();
  }

  internal Result RevealPick(string? actedByPublicId)
  {
    if (IsRevealed)
    {
      return Result.Failure(PickErrors.PickAlreadyRevealed);
    }

    RevealedAt = DateTimeOffset.UtcNow;

    _history.Add(PickEvent.Revealed(actedByPublicId: actedByPublicId));

    return Result.Success();
  }

  /// <summary>
  /// Reverses a veto on this pick. The pick is restored to active board status
  /// and the veto token is refunded to the issuer.
  /// Commissioner-only / break-glass operation.
  /// </summary>
  internal Result UndoVeto()
  {
    var current = CurrentVeto;
    if (current is null)
    {
      return Result.Failure(PickErrors.PickNotVetoed);
    }

    if (current.IsOverridden)
    {
      return Result.Failure(PickErrors.CannotUndoVetoThatHasBeenOverridden);
    }

    _vetoes.Remove(current);

    _history.Add(PickEvent.Played(issuer: null, actedByPublicId: null)); // log the undo

    return Result.Success();
  }
}

public sealed record PickEvent(
  string Kind,
  Participant? IssuerId,
  string? ActedByPublicId,
  string? Note,
  DateTime OccurredOnUtc
)
{
  public static PickEvent Played(Participant? issuer, string? actedByPublicId) =>
    new(
      Kind: "Played",
      IssuerId: issuer,
      ActedByPublicId: actedByPublicId,
      Note: null,
      OccurredOnUtc: DateTime.UtcNow
    );

  public static PickEvent Veto(Participant? issuerId, string? actedByPublicId, string? note) =>
    new(
      Kind: "Veto",
      IssuerId: issuerId,
      ActedByPublicId: actedByPublicId,
      Note: note,
      OccurredOnUtc: DateTime.UtcNow
    );

  public static PickEvent VetoOverride(Participant by, string? actedByPublicId) =>
    new(
      Kind: "VetoOverride",
      IssuerId: by,
      ActedByPublicId: actedByPublicId,
      Note: "Veto Override",
      OccurredOnUtc: DateTime.UtcNow
    );

  public static PickEvent CommissionerOverride(
    Participant by,
    string? actedByPublicId,
    string? note
  ) =>
    new(
      Kind: "CommissionerOverride",
      IssuerId: by,
      ActedByPublicId: actedByPublicId,
      Note: note,
      OccurredOnUtc: DateTime.UtcNow
    );

  public static PickEvent Revealed(string? actedByPublicId) =>
    new(
      Kind: "Revealed",
      IssuerId: null,
      ActedByPublicId: actedByPublicId,
      Note: null,
      OccurredOnUtc: DateTime.UtcNow
    );
}
