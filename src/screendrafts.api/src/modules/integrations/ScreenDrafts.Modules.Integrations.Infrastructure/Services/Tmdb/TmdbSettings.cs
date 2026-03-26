namespace ScreenDrafts.Modules.Integrations.Infrastructure.Services.Tmdb;

public sealed class TmdbSettings
{
  public const string SectionName = "Integrations:Tmdb";
  public string AccessToken { get; set; } = default!;
  public string BaseImageAddress { get; set; } = default!;
  public string BaseAddress { get; set; } = default!;
  public string TrailerPlaceholder { get; set; } = default!;
}
