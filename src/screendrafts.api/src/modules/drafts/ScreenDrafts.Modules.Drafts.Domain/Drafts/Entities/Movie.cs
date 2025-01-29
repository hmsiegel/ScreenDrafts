namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class Movie : Entity<MovieId>
{
  private Movie(
    MovieTitle movieTitle,
    MovieId? id = null) 
    : base(id ?? MovieId.CreateUnique())
  {
    MovieTitle = movieTitle;
  }

  private Movie()
  {
  }

  public MovieTitle MovieTitle { get; private set; } = default!;
  public Pick? Pick { get; private set; } = default!;

  public static Result<Movie> Create(
    MovieTitle movieTitle,
    MovieId? id = null)
  {
    if (movieTitle is null)
    {
      return Result.Failure<Movie>(MovieErrors.InvalidMovieTitle);
    }
    var movie = new Movie(
      movieTitle: movieTitle,
      id: id);
    return movie;
  }
}
