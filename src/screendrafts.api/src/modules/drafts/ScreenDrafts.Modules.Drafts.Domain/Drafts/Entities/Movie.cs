namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class Movie : Entity<MovieId>
{
  public Movie(
    MovieTitle movieTitle,
    MovieId? id = null) 
    : base(id ?? MovieId.CreateUnique())
  {
    MovieTitle = movieTitle;
  }

  public MovieTitle MovieTitle { get; private set; }
}
