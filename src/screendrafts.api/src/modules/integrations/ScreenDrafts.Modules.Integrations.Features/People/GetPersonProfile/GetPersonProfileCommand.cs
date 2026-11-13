namespace ScreenDrafts.Modules.Integrations.Features.People.GetPersonProfile;

internal sealed record GetPersonProfileCommand : ICommand<GetPersonProfileResponse>
{
  /// <summary>TMDb person ID. Zero or less means "look up by IMDb ID instead".</summary>
  public int TmdbId { get; init; }
  public string? ImdbId { get; init; }
}
