namespace ScreenDrafts.Modules.Reporting.Features.Drafts.CreateSpotlight;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<CreateSpotlightRequest, CreateSpotlightResponse>
{
  public override void Configure()
  {
    Post(ReportingRoutes.Spotlights);
    Description(x =>
    {
      x.WithTags(ReportingOpenApi.Tags.Spotlight)
        .WithName(ReportingOpenApi.Names.Spotlight_Create)
        .Produces<CreateSpotlightResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(ReportingAuth.Permissions.SpotlightCreate);
  }

  public override async Task HandleAsync(CreateSpotlightRequest req, CancellationToken ct)
  {
    var command = new CreateSpotlightCommand
    {
      DraftPublicId = req.DraftPublicId,
      SpotlightDescription = req.SpotlightDescription,
      SpotifyUrl = req.SpotifyUrl,
    };

    var result = await Sender.Send(command, ct);

    await this.SendCreatedAsync(
      result.Map(id => new CreatedIdResponse(id.SpotlightId)),
      created => ReportingLocations.ById(created.Id),
      ct
    );
  }
}
