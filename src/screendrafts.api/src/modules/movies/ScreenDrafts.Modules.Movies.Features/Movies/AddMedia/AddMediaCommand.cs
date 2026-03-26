namespace ScreenDrafts.Modules.Movies.Features.Movies.AddMedia;

internal sealed record AddMediaCommand : ICommand<string>
{
  public string PublicId { get; init; } = default!;
  public string? ImdbId { get; init; }
  public int? TmdbId { get; init; }
  public int? IgdbId { get; init; }
  public string Title { get; init; } = default!;
  public string Year { get; init; } = default!;
  public string? Plot { get; init; }
  public string? Image { get; init; }
  public string? ReleaseDate { get; init; }
  public Uri? YouTubeTrailerUrl { get; init; }
  public MediaType MediaType { get; init; } = default!;
  public int? TvSeriesTmdbId { get; init; }
  public int? SeasonNumber { get; init; }
  public int? EpisodeNumber { get; init; }
  public IReadOnlyCollection<GenreRequest> Genres { get; init; } = default!;
  public IReadOnlyCollection<PersonRequest> Directors { get; init; } = default!;
  public IReadOnlyCollection<PersonRequest> Actors { get; init; } = default!;
  public IReadOnlyCollection<PersonRequest> Writers { get; init; } = default!;
  public IReadOnlyCollection<PersonRequest> Producers { get; init; } = default!;
  public IReadOnlyCollection<ProductionCompanyRequest> ProductionCompanies { get; init; } = default!;
}

