namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class Pick : Entity<PickId>
{
  private readonly List<PickEvent> _history = [];

  private Pick(
    int position,
    Movie movie,
    Drafter? drafter,
    DrafterTeam? drafterTeam,
    DraftPart draftPart,
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

    Drafter = drafter;
    DrafterId = drafter?.Id;

    DrafterTeam = drafterTeam;
    DrafterTeamId = drafterTeam?.Id;
  }

  private Pick()
  {
  }

  public int Position { get; }
  public int PlayOrder { get; }

  public Guid MovieId { get; }
  public Movie Movie { get; } = default!;

  public DrafterId? DrafterId { get; } = default!;
  public Drafter? Drafter { get; } = default!;

  public DrafterTeamId? DrafterTeamId { get; } = default!;
  public DrafterTeam? DrafterTeam { get; } = default!;

  public DraftPartId DraftPartId { get; } = default!;
  public DraftPart DraftPart { get; } = default!;

  public DraftId DraftId { get; } = default!;
  public Draft Draft { get; } = default!;

  public Veto? Veto { get; private set; } = default!;
  public VetoId? VetoId => Veto?.Id;

  public CommissionerOverride CommissionerOverride { get; private set; } = default!;
  public Guid? CommissionerOverrideId => CommissionerOverride?.Id;


  [NotMapped]
  public bool IsVetoed => Veto is not null;

  [NotMapped]
  public bool IsComissionerOverridden => CommissionerOverride is not null;

  public IReadOnlyCollection<PickEvent> History => _history.AsReadOnly();

  public static Result<Pick> Create(
    int position,
    Movie movie,
    Drafter? drafter,
    DrafterTeam? drafterTeam,
    DraftPart draftPart,
    int playOrder,
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

    if (drafter is null && drafterTeam is null)
    {
      return Result.Failure<Pick>(PickErrors.DrafterOrTeamMustBeProvided);
    }

    if (drafter is not null && drafterTeam is not null)
    {
      return Result.Failure<Pick>(PickErrors.DrafterAndTeamCannotBeProvided);
    }


    var pick = new Pick(
      position: position,
      movie: movie,
      drafter: drafter,
      drafterTeam: drafterTeam,
      draftPart: draftPart,
      playOrder: playOrder,
      id: id);

    pick.Raise(new PickCreatedDomainEvent(
      pick.Id.Value,
      drafter?.Id.Value,
      drafterTeam?.Id.Value,
      draftPart.Id.Value,
      position,
      playOrder,
      movie.Id));

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
