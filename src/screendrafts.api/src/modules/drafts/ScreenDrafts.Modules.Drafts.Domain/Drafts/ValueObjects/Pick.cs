namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;

public sealed class Pick
{
  public int Position { get; }

  public Movie Movie { get; }

  public Drafter Drafter { get; }

  public Pick(int position, Movie movie, Drafter drafter)
  {
    if (position <= 0)
    {
      throw new ArgumentException("Position must be greater than 0.", nameof(position));
    }

    Position = position;
    Movie = Guard.Against.Null(movie);
    Drafter = Guard.Against.Null(drafter);
  }
}
