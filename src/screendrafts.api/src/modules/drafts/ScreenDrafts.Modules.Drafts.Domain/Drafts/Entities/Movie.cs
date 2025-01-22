namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class Movie(
  MovieTitle movieTitle,
  MovieId? id = null)
  : Entity<MovieId>(id ?? MovieId.CreateUnique())
{
  public MovieTitle MovieTitle { get; private set; } = movieTitle;
}
