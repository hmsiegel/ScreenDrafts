namespace ScreenDrafts.Modules.Integrations.Domain.Services.Tmdb;

public sealed record TmdbGenre
{
  public int Id { get; init; }
  public string Name { get; init; } = default!;
}
