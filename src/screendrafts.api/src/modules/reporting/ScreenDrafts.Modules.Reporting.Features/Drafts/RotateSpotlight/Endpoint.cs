namespace ScreenDrafts.Modules.Reporting.Features.Drafts.RotateSpotlight;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest
{
  public override void Configure()
  {
    Post(DraftReportingRoutes.Rotate);
    Policies(ReportingAuth.Permissions.SpotlightManage);
    Description(x =>
    {
      x.WithTags(ReportingOpenApi.Tags.Spotlight)
        .WithName(ReportingOpenApi.Names.Spotlight_RotateSpotlight)
        .Produces(StatusCodes.Status202Accepted);
    });
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var result = await Sender.Send(new RotateSpotlightCommand(), ct);
    await this.SendAcceptedAsync(result, ct);
  }
}
