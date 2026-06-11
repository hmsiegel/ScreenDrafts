namespace ScreenDrafts.Modules.Reporting.Features.Drafts.DeleteSpotlight;

internal sealed class Endpoint : ScreenDraftsEndpoint<DeleteSpotlightRequest>
{
  public override void Configure()
  {
    Delete(DraftReportingRoutes.ById);
    Description(x =>
    {
      x.WithTags(ReportingOpenApi.Tags.Spotlight)
        .WithName(ReportingOpenApi.Names.Spotlight_Delete)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(ReportingAuth.Permissions.SpotlightManage);
  }

  public override async Task HandleAsync(DeleteSpotlightRequest req, CancellationToken ct)
  {
    var command = new DeleteSpotlightCommand { PublicId = req.PublicId };
    var result = await Sender.Send(command, ct);
    await this.SendNoContentAsync(result, ct);
  }
}
