namespace ScreenDrafts.Modules.Integrations.Features.People.GetPersonFilmography;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<GetPersonFilmographyRequest, GetPersonFilmographyResponse>
{
  public override void Configure()
  {
    Get(PeopleRoutes.PersonFilmography);
    Description(x =>
    {
      x.WithTags(IntegrationsOpenApi.Tags.People)
        .WithName(IntegrationsOpenApi.Names.People_GetFilmography)
        .Produces<GetPersonFilmographyResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    });
  }

  public override async Task HandleAsync(GetPersonFilmographyRequest req, CancellationToken ct)
  {
    var command = new GetPersonFilmographyCommand { ImdbId = req.ImdbId };
    var result = await Sender.Send(command, ct);
    await this.SendOkAsync(result, ct);
  }
}
