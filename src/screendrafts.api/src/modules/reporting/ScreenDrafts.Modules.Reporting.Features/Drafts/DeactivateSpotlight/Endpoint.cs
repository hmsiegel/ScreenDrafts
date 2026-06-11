namespace ScreenDrafts.Modules.Reporting.Features.Drafts.DeactivateSpotlight;

internal sealed class Endpoint : ScreenDraftsEndpoint<DeactivateSpotlightRequest>
{
  public override void Configure()
  {
    Put(DraftReportingRoutes.Deactivate);
    Description(x =>
    {
      x.WithName(ReportingOpenApi.Names.Spotlight_Deactivate)
        .WithTags(ReportingOpenApi.Tags.Spotlight)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(ReportingAuth.Permissions.SpotlightManage);
  }

  public override async Task HandleAsync(DeactivateSpotlightRequest req, CancellationToken ct)
  {
    var command = new DeactivateSpotlightCommand { PublicId = req.PublicId };
    var result = await Sender.Send(command, ct);
    await this.SendNoContentAsync(result, ct);
  }
}
