namespace ScreenDrafts.Modules.Integrations.Domain.Imdb;

public sealed record TmdbCrewMember
{
  public int Id { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Job { get; init; } = string.Empty;
  public string Department { get; init; } = string.Empty;
}
