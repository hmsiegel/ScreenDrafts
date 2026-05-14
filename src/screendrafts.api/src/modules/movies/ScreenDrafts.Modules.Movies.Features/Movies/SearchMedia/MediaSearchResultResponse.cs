namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchMedia;

internal sealed record MediaSearchResultResponse
{
  public int? TmdbId { get; init; }
  public int? IgdbId { get; init; }
  public string Title { get; init; } = default!;
  public string? Year { get; init; }
  public string? PosterUrl { get; init; }
  public string? Overview { get; init; }
  public MediaType MediaType { get; init; } = default!;
  public bool IsInMediaDatabase { get; init; }

  /// <summary>
  /// Populated only when <see cref="IsInMediaDatabase"/> is true.
  /// Use this to link to /media/{publicId} rather than resolving by TmdbId on the frontend.
  /// </summary>
  public string? MediaPublicId { get; init; }
}
