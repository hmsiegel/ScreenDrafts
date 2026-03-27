namespace ScreenDrafts.Modules.Reporting.Domain.Movies;

public sealed class MovieHonorificHistory : Entity<MovieHonorificHistoryId>
{
  private MovieHonorificHistory(
    string moviePublicId,
    MovieHonorific appearanceHonorific,
    MoviePositionHonorific positionHonorific,
    int appearanceCount,
    DateTime achievedAt,
    MovieHonorificHistoryId? id = null)
    : base(id ?? MovieHonorificHistoryId.CreateUnique())
  {
    MoviePublicId = moviePublicId;
    AppearanceHonorific = appearanceHonorific;
    PositionHonorific = positionHonorific;
    AppearanceCount = appearanceCount;
    AchievedAt = achievedAt;
  }

  private MovieHonorificHistory() { }

  public string MoviePublicId { get; private set; } = default!;
  public MovieHonorific AppearanceHonorific { get; private set; } = MovieHonorific.None;
  public MoviePositionHonorific PositionHonorific { get; private set; } = MoviePositionHonorific.None;
  public int AppearanceCount { get; private set; }
  public DateTime AchievedAt { get; private set; }

  public static MovieHonorificHistory Create(
    string moviePublicId,
    MovieHonorific appearanceHonorific,
    MoviePositionHonorific positionHonorific,
    int appearanceCount)
  {
    return new MovieHonorificHistory(
      moviePublicId: moviePublicId,
      appearanceHonorific: appearanceHonorific,
      positionHonorific: positionHonorific,
      appearanceCount: appearanceCount,
      achievedAt: DateTime.UtcNow);
  }
}
