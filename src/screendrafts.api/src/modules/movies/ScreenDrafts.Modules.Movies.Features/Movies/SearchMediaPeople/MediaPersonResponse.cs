namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchMediaPeople;

internal sealed record MediaPersonResponse
{
  /// <summary>
  /// Identity to pass to GET /media as personKey: "t:{tmdbId}" when the person has a TMDb ID,
  /// otherwise "i:{imdbId}".
  /// </summary>
  public required string PersonKey { get; init; }
  public required string Name { get; init; }

  /// <summary>
  /// Titles in the Vault this person is credited on, within the requested role.
  /// </summary>
  public required int TitleCount { get; init; }
}
