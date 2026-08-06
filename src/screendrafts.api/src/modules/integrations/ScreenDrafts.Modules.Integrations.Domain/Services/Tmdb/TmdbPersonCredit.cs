namespace ScreenDrafts.Modules.Integrations.Domain.Services.Tmdb;

/// <summary>One entry from a person's combined movie+TV credits.</summary>
public sealed record TmdbPersonCredit
{
  public int TmdbId { get; init; }
  public string Title { get; init; } = string.Empty;
  public string? Year { get; init; }
#pragma warning disable CA1056 // URI-like properties should not be strings
  public string? PosterUrl { get; init; }
#pragma warning restore CA1056 // URI-like properties should not be strings

  /// <summary>0 = Movie, 1 = TvShow.</summary>
  public int MediaType { get; init; }

  /// <summary>Character name (cast) or job (crew) — display only.</summary>
  public string? CreditRole { get; init; }
}
