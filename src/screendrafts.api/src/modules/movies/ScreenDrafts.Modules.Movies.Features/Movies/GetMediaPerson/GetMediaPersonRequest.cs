namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaPerson;

internal sealed record GetMediaPersonRequest
{
  /// <summary>A TMDb person ID ("31644") or an IMDb person ID ("nm0000229").</summary>
  [FromRoute(Name = "id")]
  public string Id { get; init; } = default!;
}
