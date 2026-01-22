namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class Pick : Entity<PickId>
{
  private readonly List<PickEvent> _history = [];

  private Pick(
    int position,
    Movie movie,
    DraftPart draftPart,
    ParticipantId playedBy,
    string publicId,
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

    PlayedById = playedBy.Value;
    PlayedByKind = playedBy.Kind;
  }

  private Pick()
  {
  }

  public string PublicId { get; private set; } = default!;

  public int Position { get; private set; }
  public int PlayOrder { get; private set; }

  public Guid MovieId { get; private set; }
  public Movie Movie { get; private set; } = default!;

  public DraftPartId DraftPartId { get; private set; } = default!;
  public DraftPart DraftPart { get; private set; } = default!;

  public Guid PlayedById { get; private set; } 
  public ParticipantKind PlayedByKind { get; private set; } = default!;

  public ParticipantId PlayedBy => new(PlayedById, PlayedByKind);


  public Veto? Veto { get; private set; } = default!;
  public VetoId? VetoId => Veto?.Id;


  public CommissionerOverride CommissionerOverride { get; private set; } = default!;
  public Guid? CommissionerOverrideId => CommissionerOverride?.Id;

  public bool IsActiveOnFinalBoard => !IsVetoed && !IsComissionerOverridden;


  [NotMapped]
  public bool IsVetoed => Veto is not null;

  [NotMapped]
  public bool IsComissionerOverridden => CommissionerOverride is not null;

  public IReadOnlyCollection<PickEvent> History => _history.AsReadOnly();

  internal static Result<Pick> Create(
    int position,
    Movie movie,
    ParticipantId playedBy,
    DraftPart draftPart,
    int playOrder,
    string publicId,
    PickId? id = null)
  {
    if (draftPart is null)
    {
      return Result.Failure<Pick>(PickErrors.DraftMustBeProvided);
    }

    if (position <= 0 || position > draftPart.Draft.TotalPicks)
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

    if (playedBy.Kind is null)
    {
      return Result.Failure<Pick>(PickErrors.DrafterOrTeamMustBeProvided);
    }

    var pick = new Pick(
      position: position,
      movie: movie,
      playedBy: playedBy,
      draftPart: draftPart,
      playOrder: playOrder,
      publicId: publicId,
      id: id);

    pick.Raise(new PickCreatedDomainEvent(
      pickId: pick.Id.Value,
      participantId: playedBy.Value,
      participantKind: playedBy.Kind.Name,
      draftPartId: draftPart.Id.Value,
      position: position,
      playOrder: playOrder,
      movieId: movie.Id));

    return pick;
  }

  internal Result ApplyVeto(Veto veto)
  {
    if (veto is null)
    {
      return Result.Failure(PickErrors.VetoMustBeProvided);
    }

    Guard.Against.Null(veto);

    if (IsVetoed)
    {
      return Result.Failure(PickErrors.PickAlreadyVetoed);
    }

    Veto = veto;

    _history.Add(
      PickEvent.Veto(
        issuerKind: veto.IssuerKind,
        issuerId: veto.IssuedBy,
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

  internal Result ApplyVetoOverride(Veto veto, ParticipantId by)
  {
    if (veto is null)
    {
      return Result.Failure(PickErrors.VetoMustBeProvided);
    }

    Guard.Against.Null(veto);

    if (IsVetoed)
    {
      return Result.Failure(PickErrors.PickAlreadyVetoed);
    }

    Veto = veto;

    _history.Add(
      PickEvent.VetoOverride(
        by: by,
        note: veto.Note));

    return Result.Success();
  }
}

public sealed record PickEvent(
  string Kind,
  VetoIssuerKind? IssuerKind,
  ParticipantId? IssuerId,
  string? Note,
  DateTime OccurredOnUtc)
{
  public static PickEvent Veto(
    VetoIssuerKind issuerKind,
    ParticipantId? issuerId,
    string? note) =>
    new(
      Kind: "Veto",
      IssuerKind: issuerKind,
      IssuerId: issuerId,
      Note: note,
      OccurredOnUtc: DateTime.UtcNow);

  public static PickEvent VetoOverride(
    ParticipantId by,
    string? note) =>
    new(
      Kind: "VetoOverride",
      IssuerKind: null,
      IssuerId: by,
      Note: note,
      OccurredOnUtc: DateTime.UtcNow);

  public static PickEvent CommissionerOverride(
    ParticipantId by,
    string? note) =>
    new(
      Kind: "CommissionerOverride",
      IssuerKind: null,
      IssuerId: by,
      Note: note,
      OccurredOnUtc: DateTime.UtcNow);
}
