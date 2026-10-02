namespace ScreenDrafts.Modules.Drafts.Features.WikiExport.ExportDrafts;

// ── Endpoint ──────────────────────────────────────────────────────────────

internal sealed class Endpoint : ScreenDraftsEndpoint<ExportDraftsWikiRequest, ExportWikiResponse>
{
  public override void Configure()
  {
    Post(WikiExportRoutes.Drafts);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.WikiExport)
        .WithName(DraftsOpenApi.Names.WikiExport_Drafts)
        .Produces<ExportWikiResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.WikiExport);
  }

  public override async Task HandleAsync(ExportDraftsWikiRequest req, CancellationToken ct)
  {
    var query = new ExportDraftsWikiQuery { DraftPublicIds = req.DraftPublicIds };
    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}
