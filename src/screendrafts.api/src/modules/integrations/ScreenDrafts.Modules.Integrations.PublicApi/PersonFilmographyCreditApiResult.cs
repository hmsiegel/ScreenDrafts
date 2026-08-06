namespace ScreenDrafts.Modules.Integrations.PublicApi;

public sealed record PersonFilmographyCreditApiResult
{
  public int TmdbId { get; init; }
  public string Title { get; init; } = default!;
  public string? Year { get; init; }
  public string? PosterPath { get; init; }

  /// <summary>0 = Movie, 1 = TvShow.</summary>
  public int MediaType { get; init; }
  public string? CreditRole { get; init; }
}
