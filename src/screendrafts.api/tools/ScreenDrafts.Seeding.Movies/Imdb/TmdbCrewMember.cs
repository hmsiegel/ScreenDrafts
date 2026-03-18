namespace ScreenDrafts.Seeding.Movies.Imdb;

public sealed record TmdbCrewMember
{
  public int TmdbId { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Job { get; init; } = string.Empty;
  public string Department { get; init; } = string.Empty;
  public string? ImdbId { get; init; }
}
