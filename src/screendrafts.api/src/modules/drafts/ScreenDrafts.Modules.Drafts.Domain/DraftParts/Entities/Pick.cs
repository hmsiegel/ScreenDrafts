namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;

public sealed class Pick : Entity<PickId>
{
  private readonly List<PickEvent> _history = [];

  private Pick(
    int position,
    Movie movie,
    DraftPart draftPart,
    DraftPartParticipant playedByParticipant,
    int playOrder = 0,
    PickId? id = null)
    : base(id ?? PickId.CreateUnique())
  {
    Position = position;
    PlayOrder = playOrder;

    Movie = Guard.Against.Null(movie);
    MovieId = movie.Id;

    DraftPart = Guard.Against.Null(draftPart);
    DraftPartId = draftPart.Id;

    PlayedByParticipant = playedByParticipant;
    PlayedByParticipantId = playedByParticipant.Id;

    PlayedByParticipantIdValue = playedByParticipant.ParticipantIdValue;
    PlayedByParticipantKindValue = playedByParticipant.ParticipantKindValue;
  }

  private Pick()
  {
  }

  public int Position { get; private set; }
  public int PlayOrder { get; private set; }

  public Guid MovieId { get; private set; }
  public Movie Movie { get; private set; } = default!;

  public string? MovieVersionName { get; private set; } = default!;

  public DraftPartId DraftPartId { get; private set; } = default!;
  public DraftPart DraftPart { get; private set; } = default!;

  public DraftPartParticipant PlayedByParticipant { get; private set; } = default!;
  public DraftPartParticipantId PlayedByParticipantId { get; private set; } = default!;

  public Guid PlayedByParticipantIdValue { get ; private set; }
  public ParticipantKind PlayedByParticipantKindValue { get; private set; } = default!;


  public Veto? Veto { get; private set; } = default!;

  [NotMapped]
  public VetoId? VetoId => Veto?.Id;


  public CommissionerOverride? CommissionerOverride { get; private set; } = default!;

  [NotMapped]
  public Guid? CommissionerOverrideId => CommissionerOverride?.Id;

  public bool IsActiveOnFinalBoard => !IsVetoed && !IsCommissionerOverridden;


  [NotMapped]
  public bool IsVetoed => Veto is not null && !Veto.IsOverridden;

  [NotMapped]
  public bool IsCommissionerOverridden => CommissionerOverride is not null;

  public IReadOnlyCollection<PickEvent> History => _history.AsReadOnly();

  internal static Result<Pick> Create(
    int position,
    Movie movie,
    DraftPartParticipant playedByParticipant,
    DraftPart draftPart,
    int playOrder,
    string? movieVersionName = null,
    MovieVersionPolicy? versionPolicy = null,
    PickId? id = null)
  {
    if (draftPart is null)
    {
      return Result.Failure<Pick>(PickErrors.DraftMustBeProvided);
    }

    if (!draftPart.MinPosition.HasValue ||
          !draftPart.MaxPosition.HasValue ||
          draftPart.MinPosition.Value <= 0 ||
          draftPart.MaxPosition.Value <= 0)
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
      playOrder: playOrder,
      id: id);

    var setVersionResult = pick.SetMovieVersionName(
      movieVersionName: movieVersionName,
      movie: movie,
      versionPolicy: versionPolicy);

    if (setVersionResult.IsFailure)
    {
      return Result.Failure<Pick>(setVersionResult.Errors);
    }

    pick.Raise(new PickCreatedDomainEvent(
      pickId: pick.Id.Value,
      participantId: playedByParticipant.ParticipantIdValue,
      participantKind: playedByParticipant.ParticipantKindValue.Name,
      draftPartId: draftPart.Id.Value,
      position: position,
      playOrder: playOrder,
      movieId: movie.Id));

    return pick;
  }

  internal static Result<Pick> SeedCreate(
    int position,
    Movie movie,
    DraftPart draftPart,
    DraftPartParticipant playedByParticipant,
    int playOrder,
    string? movieVersionName = null,
    PickId? id = null)
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
      position,
      movie,
      draftPart,
      playedByParticipant,
      playOrder,
      id);

    var versionResult = pick.SetMovieVersionName(movieVersionName, movie, versionPolicy: null);

    if (versionResult.IsFailure)
    {
      return Result.Failure<Pick>(versionResult.Errors);
    }

    return pick;
  }

  internal Result SetMovieVersionName(string? movieVersionName, Movie movie, MovieVersionPolicy? versionPolicy = null)
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

    var trimmed = movieVersionName!.Trim();

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

  internal Result ApplyVeto(Veto veto)
  {
    if (IsVetoed)
    {
      return Result.Failure(PickErrors.PickAlreadyVetoed);
    }

    Veto = veto;

    _history.Add(
      PickEvent.Veto(
        issuerId: veto.IssuedByParticipant.ParticipantId,
        note: veto.Note));

    return Result.Success();
  }

  internal Result ApplyCommissionerOverride(
    CommissionerOverride commissionerOverride)
  {
    Guard.Against.Null(commissionerOverride);

    if (CommissionerOverride is not null)
    {
      return Result.Failure(PickErrors.CommissionerOverrideAlreadyApplied);
    }

    CommissionerOverride = commissionerOverride;

    Raise(
      new CommissionerOverrideAppliedDomainEvent(
        commissionerOverrideId: commissionerOverride.Id,
        pickId: Id.Value));

    return Result.Success();
  }

  internal Result ApplyVetoOverride(ParticipantId by)
  {
    if (Veto is null || !IsVetoed)
    {
      return Result.Failure(PickErrors.CannotOverrideAPickThatHasNotBeenVetoed);
    }

    var result = Veto.Override(by);

    if (result.IsFailure)
    {
      return result;
    }

    _history.Add(
      PickEvent.VetoOverride(
        by: by));

    return Result.Success();
  }
}

public sealed record PickEvent(
  string Kind,
  ParticipantId? IssuerId,
  string? Note,
  DateTime OccurredOnUtc)
{
  public static PickEvent Veto(
    ParticipantId? issuerId,
    string? note) =>
    new(
      Kind: "Veto",
      IssuerId: issuerId,
      Note: note,
      OccurredOnUtc: DateTime.UtcNow);

  public static PickEvent VetoOverride(
    ParticipantId by) =>
    new(
      Kind: "VetoOverride",
      IssuerId: by,
      Note: "Veto Override",
      OccurredOnUtc: DateTime.UtcNow);

  public static PickEvent CommissionerOverride(
    ParticipantId by,
    string? note) =>
    new(
      Kind: "CommissionerOverride",
      IssuerId: by,
      Note: note,
      OccurredOnUtc: DateTime.UtcNow);
}
