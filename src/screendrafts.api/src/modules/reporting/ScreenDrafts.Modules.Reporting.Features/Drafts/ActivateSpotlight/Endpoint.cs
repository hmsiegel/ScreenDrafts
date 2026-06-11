namespace ScreenDrafts.Modules.Reporting.Features.Drafts.ActivateSpotlight;

internal sealed class Endpoint : ScreenDraftsEndpoint<ActivateSpotlightRequest>
{
  public override void Configure()
  {
    Put(DraftReportingRoutes.ById);
    Description(x =>
    {
      x.WithName(ReportingOpenApi.Names.Spotlight_Activate)
        .WithTags(ReportingOpenApi.Tags.Spotlight)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(ReportingAuth.Permissions.SpotlightManage);
  }

  public override async Task HandleAsync(ActivateSpotlightRequest req, CancellationToken ct)
  {
    var command = new ActivateSpotlightCommand { PublicId = req.PublicId };
    var result = await Sender.Send(command, ct);
    await this.SendNoContentAsync(result, ct);
  }
}
