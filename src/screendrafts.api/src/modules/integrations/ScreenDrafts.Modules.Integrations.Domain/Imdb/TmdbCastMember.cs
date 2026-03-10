namespace ScreenDrafts.Modules.Integrations.Domain.Imdb;

public sealed record TmdbCastMember
{
  public int Id { get; init; }
  public string Name { get; init; } = string.Empty;
  public string? KnownForDepartment { get; init; }
}
