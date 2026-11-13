namespace ScreenDrafts.Modules.Movies.Features.Movies.StartMediaPeopleResync;

internal sealed record StartMediaPeopleResyncRequest
{
  /// <summary>Stop after this many titles. Leave out to process every pending title.</summary>
  [FromQuery(Name = "limit")]
  public int? Limit { get; init; }
}
