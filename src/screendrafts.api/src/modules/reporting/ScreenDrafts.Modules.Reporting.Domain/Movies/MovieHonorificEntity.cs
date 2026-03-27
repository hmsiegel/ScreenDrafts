namespace ScreenDrafts.Modules.Reporting.Domain.Movies;

public sealed class MovieHonorificEntity : Entity<MovieHonorificId>
{
  private MovieHonorificEntity(
    string moviePublicId,
    string movieTitle,
    MovieHonorific appearanceHonorific,
    MoviePositionHonorific positionHonorific,
    int appearanceCount,
    DateTime updateAtUtc,
    MovieHonorificId? id = null)
    : base(id ?? MovieHonorificId.CreateUnique())
  {
    MoviePublicId = moviePublicId;
    MovieTitle = movieTitle;
    AppearanceHonorific = appearanceHonorific;
    PositionHonorific = positionHonorific;
    AppearanceCount = appearanceCount;
    UpdateAtUtc = updateAtUtc;
  }

  private MovieHonorificEntity()
  {

  }

  public string MoviePublicId { get; private set; } = default!;
  public string MovieTitle { get; private set; } = default!;
  public MovieHonorific AppearanceHonorific { get; private set; } = MovieHonorific.None;
  public MoviePositionHonorific PositionHonorific { get; private set; } = MoviePositionHonorific.None;
  public int AppearanceCount { get; private set; }
  public DateTime UpdateAtUtc { get; private set; }

  public static MovieHonorificEntity Create(
    string moviePublicId,
    string movieTitle,
    MovieHonorific appearanceHonorific,
    MoviePositionHonorific positionHonorific,
    int appearanceCount)
  {
    return new MovieHonorificEntity(
      moviePublicId: moviePublicId,
      movieTitle: movieTitle,
      appearanceHonorific: appearanceHonorific,
      positionHonorific: positionHonorific,
      appearanceCount: appearanceCount,
      updateAtUtc: DateTime.UtcNow);
  }
}
