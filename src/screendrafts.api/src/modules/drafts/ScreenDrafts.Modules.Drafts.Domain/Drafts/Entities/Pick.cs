namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class Pick
{
  private Pick(
    int position,
    Movie movie,
    Drafter drafter,
    Guid? id = null)
  {

    Position = position;
    Movie = Guard.Against.Null(movie);
    Drafter = Guard.Against.Null(drafter);
    Id = id ?? Guid.NewGuid();
  }

  private Pick()
  {
  }

  public Guid Id { get; init; }

  public int Position { get; }

  public MovieId MovieId { get; } = default!;

  public Movie Movie { get; } = default!;

  public VetoId? VetoId { get; private set; } = default!;

  public Veto? Veto { get; private set; } = default!;

  public DrafterId DrafterId { get; } = default!;

  public Drafter Drafter { get; } = default!;


  public static Result<Pick> Create(
    int position,
    Movie movie,
    Drafter drafter,
    Guid? id = null)
  {
    if (position <= 0)
    {
      return Result.Failure<Pick>(DraftErrors.InvalidPickPosition);
    }

    var pick = new Pick(
      position: position,
      movie: movie,
      drafter: drafter,
      id: id);

    return pick;
  }
}
