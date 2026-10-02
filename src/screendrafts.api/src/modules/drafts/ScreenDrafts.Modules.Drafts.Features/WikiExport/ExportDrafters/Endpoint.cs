namespace ScreenDrafts.Modules.Drafts.Features.WikiExport.ExportDrafters;

// ── Endpoint ──────────────────────────────────────────────────────────────

internal sealed class Endpoint : ScreenDraftsEndpoint<ExportDraftersWikiRequest, ExportWikiResponse>
{
  public override void Configure()
  {
    Post(WikiExportRoutes.Drafters);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.WikiExport)
        .WithName(DraftsOpenApi.Names.WikiExport_Drafters)
        .Produces<ExportWikiResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.WikiExport);
  }

  public override async Task HandleAsync(ExportDraftersWikiRequest req, CancellationToken ct)
  {
    var query = new ExportDraftersWikiQuery { DrafterPublicIds = req.DrafterPublicIds };
    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}
