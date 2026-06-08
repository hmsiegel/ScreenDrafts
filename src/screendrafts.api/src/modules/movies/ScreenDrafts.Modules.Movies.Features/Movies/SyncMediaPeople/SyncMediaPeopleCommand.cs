namespace ScreenDrafts.Modules.Movies.Features.Movies.SyncMediaPeople;

internal sealed record SyncMediaPeopleCommand : ICommand
{
  public int TmdbId { get; init; }
  public MediaType MediaType { get; init; } = default!;
  public int? TvSeriesTmdbId { get; init; }
  public int? SeasonNumber { get; init; }
  public int? EpisodeNumber { get; init; }
  public IReadOnlyCollection<PersonRequest> Directors { get; init; } = [];
  public IReadOnlyCollection<PersonRequest> Actors { get; init; } = [];
  public IReadOnlyCollection<PersonRequest> Writers { get; init; } = [];
  public IReadOnlyCollection<PersonRequest> Producers { get; init; } = [];
  public IReadOnlyCollection<ProductionCompanyRequest> ProductionCompanies { get; init; } = [];
}
