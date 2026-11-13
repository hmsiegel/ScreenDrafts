namespace ScreenDrafts.Modules.Integrations.Features.People.GetPersonProfile;

internal sealed record GetPersonProfileResponse
{
  public int TmdbId { get; init; }
  public string Name { get; init; } = string.Empty;
  public string? PhotoUrl { get; init; }
}
