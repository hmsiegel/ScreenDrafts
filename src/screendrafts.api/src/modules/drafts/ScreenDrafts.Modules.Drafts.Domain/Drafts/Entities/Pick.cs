namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class Pick : Entity<PickId>
{
  private Pick(
    int position,
    Movie movie,
    Drafter? drafter,
    DrafterTeam? drafterTeam,
    Draft draft,
    int playOrder = 0,
    PickId? id = null)
    : base(id ?? PickId.CreateUnique())
  {
    Position = position;
    PlayOrder = playOrder;

    Movie = Guard.Against.Null(movie);
    MovieId = movie.Id;

    Draft = Guard.Against.Null(draft);
    DraftId = draft.Id;

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

  public DraftId DraftId { get; } = default!;
  public Draft Draft { get; } = default!;

  public Veto? Veto { get; private set; } = default!;
  public VetoId? VetoId => Veto?.Id;

  public CommissionerOverride CommissionerOverride { get; private set; } = default!;
  public Guid? CommissionerOverrideId => CommissionerOverride?.Id;


  [NotMapped]
  public bool IsVetoed => Veto is not null;

  public static Result<Pick> Create(
    int position,
    Movie movie,
    Drafter? drafter,
    DrafterTeam? drafterTeam,
    Draft draft,
    int playOrder,
    PickId? id = null)
  {
    if (draft is null)
    {
      return Result.Failure<Pick>(PickErrors.DraftMustBeProvided);
    }

    if (position <= 0 || position > draft.TotalPicks)
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
      draft: draft,
      playOrder: playOrder,
      id: id);

    pick.Raise(new PickCreatedDomainEvent(
      pick.Id.Value,
      drafter?.Id.Value,
      drafterTeam?.Id.Value,
      draft.Id.Value,
      position,
      playOrder,
      movie.Id));

    return pick;
  }

  public Result VetoPick(Veto veto)
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

    return Result.Success();
  }

  public Result ApplyCommissionerOverride(
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
}
