namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class Movie : Entity
{
  private readonly List<Pick> _picks = [];
  private Movie(
    string movieTitle,
    Guid? id = null) 
    : base(id ?? Guid.NewGuid())
  {
    MovieTitle = movieTitle;
  }

  private Movie()
  {
  }

  public IReadOnlyCollection<Pick> Picks => _picks.AsReadOnly();

  public string MovieTitle { get; private set; } = default!;

  public static Result<Movie> Create(
    string movieTitle,
    Guid? id = null)
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
