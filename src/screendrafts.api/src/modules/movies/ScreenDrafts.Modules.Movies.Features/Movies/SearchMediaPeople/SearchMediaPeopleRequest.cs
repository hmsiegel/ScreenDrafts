namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchMediaPeople;

internal sealed record SearchMediaPeopleRequest
{
  /// <summary>
  /// Part of the person's name. Fewer than two characters returns an empty list.
  /// </summary>
  [FromQuery(Name = "query")]
  public string? Query { get; init; }

  /// <summary>
  /// any (default), actor, director, writer, or producer.
  /// </summary>
  [FromQuery(Name = "role")]
  public string? Role { get; init; }

  [FromQuery(Name = "limit")]
  public int Limit { get; init; } = 10;
}
