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
    if (position <= 0)
    {
      return Result.Failure<Pick>(DraftErrors.PickPositionIsOutOfRange);
    }

    var pick = new Pick(
      position: position,
      movie: movie,
      drafter: drafter,
      drafterTeam: drafterTeam,
      draft: draft,
      playOrder: playOrder,
      id: id);

    return pick;
  }

  public Result VetoPick(Veto veto)
  {
    Guard.Against.Null(veto);

    if (IsVetoed)
    {
      return Result.Failure(DraftErrors.PickAlreadyVetoed);
    }

    Veto = veto;

    return Result.Success();
  }
}
