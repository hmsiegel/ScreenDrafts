namespace ScreenDrafts.Modules.Integrations.Domain.Services.Tmdb;

public sealed record TmdbCredits
{
  public IReadOnlyList<TmdbCastMember> Cast { get; init; } = default!;
  public IReadOnlyList<TmdbCrewMember> Crew { get; init; } = default!;
}
