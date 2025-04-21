namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class Movie : Entity
{
  private readonly List<Pick> _picks = [];
  private Movie(
    string movieTitle,
    string imdbId,
    Guid id) 
    : base(id)
  {
    MovieTitle = movieTitle;
    ImdbId = imdbId;
  }

  private Movie()
  {
  }

  public IReadOnlyCollection<Pick> Picks => _picks.AsReadOnly();

  public string MovieTitle { get; private set; } = default!;
  public string ImdbId { get; private set; } = default!;

  public static Result<Movie> Create(
    string movieTitle,
    string imdbId,
    Guid id)
  {
    if (string.IsNullOrWhiteSpace(movieTitle))
    {
      return Result.Failure<Movie>(MovieErrors.InvalidMovieTitle);
    }

    if (string.IsNullOrWhiteSpace(imdbId))
    {
      return Result.Failure<Movie>(MovieErrors.InvalidImdbId);
    }

    var movie = new Movie(
      movieTitle: movieTitle,
      imdbId: imdbId,
      id: id);
    return movie;
  }
}
