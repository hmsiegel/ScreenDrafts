namespace ScreenDrafts.Modules.Integrations.Domain.Services.Tmdb;

public sealed record TmdbProductionCompany
{
  public int Id { get; init; }
  public string Name { get; init; } = string.Empty;
}
