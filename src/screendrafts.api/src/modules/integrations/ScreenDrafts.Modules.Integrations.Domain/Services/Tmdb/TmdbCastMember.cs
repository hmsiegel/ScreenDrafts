namespace ScreenDrafts.Modules.Integrations.Domain.Services.Tmdb;

public sealed record TmdbCastMember
{
  public int TmdbId { get; init; }
  public string Name { get; init; } = string.Empty;
  public string? KnownForDepartment { get; init; }
  public string? ImdbId { get; init; }  
}
