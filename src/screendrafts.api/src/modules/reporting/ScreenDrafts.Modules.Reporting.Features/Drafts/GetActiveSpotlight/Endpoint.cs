namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetActiveSpotlight;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest<GetActiveSpotlightResponse>
{
  public override void Configure()
  {
    Get(ReportingRoutes.Spotlight);
    Description(x =>
    {
      x.WithTags(ReportingOpenApi.Tags.Spotlight)
        .WithName(ReportingOpenApi.Names.Spotlight_GetActive)
        .Produces<GetActiveSpotlightResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var query = new GetActiveSpotlightQuery();

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
