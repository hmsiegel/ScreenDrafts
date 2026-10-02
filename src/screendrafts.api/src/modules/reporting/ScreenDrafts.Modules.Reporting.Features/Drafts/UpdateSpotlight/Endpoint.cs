namespace ScreenDrafts.Modules.Reporting.Features.Drafts.UpdateSpotlight;

// ── Endpoint ──────────────────────────────────────────────────────────────

internal sealed class Endpoint : ScreenDraftsEndpoint<UpdateSpotlightRequest>
{
  public override void Configure()
  {
    Put(DraftReportingRoutes.ById);
    Description(x =>
    {
      x.WithName(ReportingOpenApi.Names.Spotlight_Update)
        .WithTags(ReportingOpenApi.Tags.Spotlight)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(ReportingAuth.Permissions.SpotlightManage);
  }

  public override async Task HandleAsync(UpdateSpotlightRequest req, CancellationToken ct)
  {
    var command = new UpdateSpotlightCommand
    {
      PublicId = req.PublicId,
      SpotlightDescription = req.SpotlightDescription,
      SpotifyUrl = req.SpotifyUrl,
    };

    var result = await Sender.Send(command, ct);
    await this.SendNoContentAsync(result, ct);
  }
}
