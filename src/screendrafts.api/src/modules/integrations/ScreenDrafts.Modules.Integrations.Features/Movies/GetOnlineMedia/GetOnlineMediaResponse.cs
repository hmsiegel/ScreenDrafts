namespace ScreenDrafts.Modules.Integrations.Features.Movies.GetOnlineMedia;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "<Pending>")]
internal sealed record GetOnlineMediaResponse
{
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
  public List<GenreModel> Genres { get; init; } = default!;
  public List<ActorModel> Actors { get; init; } = default!;
  public List<DirectorModel> Directors { get; init; } = default!;
  public List<WriterModel> Writers { get; init; } = default!;
  public List<ProducerModel> Producers { get; init; } = default!;
  public List<ProductionCompanyModel> ProductionCompanies { get; init; } = default!;
}
