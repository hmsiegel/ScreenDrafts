namespace ScreenDrafts.Modules.Movies.Features.Movies.AddMedia;

internal sealed record AddMediaRequest
{
  public required string PublicId { get; init; }
  public string? ImdbId { get; init; }
  public int? TmdbId { get; init; }
  public int? IgdbId { get; init; }
  public required string Title { get; init; }
  public required string Year { get; init; }
  public string? Plot { get; init; }
  public string? Image { get; init; }
  public string? ReleaseDate { get; init; }
  public Uri? YoutubeTrailerUrl { get; init; }
  public MediaType MediaType { get; init; } = default!;
  public int? TvSeriesTmdbId {  get; init; }
  public int? SeasonNumber { get; init; }
  public int? EpisodeNumber { get; init; }
  public IReadOnlyCollection<GenreRequest> Genres { get; init; } = [];
  public IReadOnlyCollection<PersonRequest> Actors { get; init; } = [];
  public IReadOnlyCollection<PersonRequest> Directors { get; init; } = [];
  public IReadOnlyCollection<PersonRequest> Writers { get; init; } = [];
  public IReadOnlyCollection<PersonRequest> Producers { get; init; } = [];
  public IReadOnlyCollection<ProductionCompanyRequest> ProductionCompanies { get; init; } = [];
}

