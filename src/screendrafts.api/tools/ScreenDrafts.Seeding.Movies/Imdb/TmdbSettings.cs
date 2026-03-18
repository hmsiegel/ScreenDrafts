namespace ScreenDrafts.Seeding.Movies.Imdb;

public sealed class TmdbSettings
{
  public const string SectionName = "Tmdb";
  public string AccessToken { get; set; } = default!;
  public string BaseImageAddress { get; set; } = default!;
  public string BaseAddress { get; set; } = default!;
  public string TrailerPlaceholder { get; set; } = default!;
}
