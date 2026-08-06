namespace ScreenDrafts.Modules.Integrations.Features.People.GetPersonFilmography;

internal sealed record GetPersonFilmographyCommand : ICommand<GetPersonFilmographyResponse>
{
  public required string ImdbId { get; init; }
}
