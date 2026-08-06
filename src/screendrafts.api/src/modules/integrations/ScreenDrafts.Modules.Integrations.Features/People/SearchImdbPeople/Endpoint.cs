namespace ScreenDrafts.Modules.Integrations.Features.People.SearchImdbPeople;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<SearchImdbPeopleRequest, SearchImdbPeopleResponse>
{
  public override void Configure()
  {
    Get(PeopleRoutes.PeopleSearch);
    Description(x =>
    {
      x.WithTags(IntegrationsOpenApi.Tags.People)
        .WithName(IntegrationsOpenApi.Names.People_Search)
        .Produces<SearchImdbPeopleResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    });
  }

  public override async Task HandleAsync(SearchImdbPeopleRequest req, CancellationToken ct)
  {
    var command = new SearchImdbPeopleCommand { Query = req.Query };
    var result = await Sender.Send(command, ct);
    await this.SendOkAsync(result, ct);
  }
}
