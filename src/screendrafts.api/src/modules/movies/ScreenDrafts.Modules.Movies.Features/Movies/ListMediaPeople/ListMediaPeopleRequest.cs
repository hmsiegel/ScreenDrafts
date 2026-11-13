namespace ScreenDrafts.Modules.Movies.Features.Movies.ListMediaPeople;

internal sealed record ListMediaPeopleRequest
{
  [FromQuery]
  public int Page { get; init; } = 1;

  [FromQuery]
  public int PageSize { get; init; } = 48;

  /// <summary>Part of the person's name.</summary>
  [FromQuery]
  public string? Search { get; init; }

  /// <summary>any (default), actor, director, writer, or producer.</summary>
  [FromQuery]
  public string? Role { get; init; }

  /// <summary>name_asc (default), name_desc, titles_desc, titles_asc.</summary>
  [FromQuery]
  public string? Sort { get; init; }
}
